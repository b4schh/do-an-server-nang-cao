using DoAn.Core.Application.Interfaces.Recommendation;
using DoAn.Core.Application.DTOs.Recommendation;
using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Core.Application.Interfaces.Review;

namespace DoAn.Core.Application.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private readonly IComplexRepository _complexRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IReviewRepository _reviewRepository;

    public RecommendationService(
        IComplexRepository complexRepository,
        IBookingRepository bookingRepository,
        IReviewRepository reviewRepository)
    {
        _complexRepository = complexRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
    }

    #region Item-to-Item Recommendation (Complex tương tự)

    /// <summary>
    /// STRATEGY 1: Item-to-Item Similarity at Complex level
    /// Gợi ý Complex tương tự dựa trên vector đặc trưng
    /// </summary>
    public async Task<RecommendationResponse> GetSimilarComplexesAsync(int complexId, int topK = 10)
    {
        // Lấy Complex hiện tại với đầy đủ thông tin
        var currentComplex = await _complexRepository.GetComplexWithDetailsForRecommendationAsync(complexId);

        if (currentComplex == null)
        {
            return new RecommendationResponse
            {
                RecommendationType = "complex-similarity",
                Complexes = new List<ComplexRecommendationDto>(),
                Message = "Không tìm thấy cụm sân"
            };
        }

        // Vector hóa Complex hiện tại
        var currentVector = VectorizeComplex(currentComplex);

        // Lấy tất cả Complex khác (cùng tỉnh để tăng độ chính xác)
        var otherComplexes = (await _complexRepository.GetAllActiveComplexesWithDetailsAsync(currentComplex.Province))
            .Where(c => c.Id != complexId)
            .ToList();

        // Tính similarity cho từng Complex
        var similarities = new List<(ComplexEntity complex, double score, int randomOrder)>();
        var random = new Random();
        
        foreach (var complex in otherComplexes)
        {
            var complexVector = VectorizeComplex(complex);
            var similarity = CosineSimilarity(currentVector, complexVector);
            similarities.Add((complex, similarity, random.Next()));
        }

        // Lấy Top-K Complex có similarity cao nhất
        var topComplexes = similarities
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.randomOrder)
            .Take(topK)
            .Select(x => MapToRecommendationDto(x.complex, x.score))
            .ToList();

        return new RecommendationResponse
        {
            RecommendationType = "complex-similarity",
            Complexes = topComplexes,
            Message = $"Tìm thấy {topComplexes.Count} cụm sân tương tự"
        };
    }

    #endregion

    #region Location-based + Popularity Recommendation (User mới)

    /// <summary>
    /// STRATEGY 2: Location-based + Popularity
    /// Gợi ý Complex cho user mới dựa trên vị trí và độ phổ biến
    /// </summary>
    public async Task<RecommendationResponse> GetRecommendationsForNewUserAsync(
        string? province, 
        string? ward, 
        int topK = 10)
    {
        // Lấy tất cả Complex active
        var complexes = (await _complexRepository.GetAllActiveComplexesWithDetailsAsync(province)).ToList();

        // Lọc theo ward nếu có
        if (!string.IsNullOrEmpty(ward))
        {
            complexes = complexes.Where(c => c.Ward == ward).ToList();
        }

        if (!complexes.Any())
        {
            return new RecommendationResponse
            {
                RecommendationType = "location-popularity",
                Complexes = new List<ComplexRecommendationDto>(),
                Message = "Không tìm thấy cụm sân phù hợp"
            };
        }

        // Query booking counts directly from database (OPTIMIZED)
        var complexIds = complexes.Select(c => c.Id).ToList();
        var bookingCounts = await _bookingRepository.GetComplexBookingCountsAsync(complexIds);
        
        // Tính popularity score cho mỗi Complex
        var random = new Random();
        var scoredComplexes = complexes.Select(complex => 
        {
            var totalBookings = bookingCounts.ContainsKey(complex.Id) ? bookingCounts[complex.Id] : 0;
            var avgRating = GetAverageRating(complex.Id);
            
            // Công thức popularity: 0.6 * normalized_booking + 0.4 * normalized_rating
            var maxBooking = bookingCounts.Values.Any() ? bookingCounts.Values.Max() : 0;
            var normalizedBooking = maxBooking > 0 ? (double)totalBookings / maxBooking : 0;
            var normalizedRating = avgRating / 5.0;
            
            var popularityScore = 0.6 * normalizedBooking + 0.4 * normalizedRating;
            
            return (complex, popularityScore, randomOrder: random.Next());
        })
        .OrderByDescending(x => x.popularityScore)
        .ThenByDescending(x => x.randomOrder)
        .Take(topK)
        .Select(x => MapToRecommendationDto(x.complex, x.popularityScore))
        .ToList();

        return new RecommendationResponse
        {
            RecommendationType = "location-popularity",
            Complexes = scoredComplexes,
            Message = $"Gợi ý {scoredComplexes.Count} cụm sân phổ biến"
        };
    }

    #endregion

    #region Content-based User Recommendation (User có lịch sử)

    /// <summary>
    /// STRATEGY 3: Content-Based Filtering with 3-Tier Location Priority
    /// Gợi ý cá nhân hóa dựa trên lịch sử booking của user
    /// 3-TIER SYSTEM: Same ward (100%) > Same province (85%) > Other province (60%)
    /// </summary>
    public async Task<RecommendationResponse> GetPersonalizedRecommendationsAsync(
        int userId, 
        int topK = 10, 
        string? province = null,
        string? ward = null)
    {
        // Lấy lịch sử booking của user
        var userBookings = (await _bookingRepository.GetUserBookingHistoryAsync(userId)).ToList();

        if (!userBookings.Any())
        {
            // Fallback to location-based nếu user chưa có booking
            return await GetRecommendationsForNewUserAsync(province, ward, topK);
        }

        // Tạo user vector từ các Complex đã đặt (KHÔNG bao gồm location features)
        var bookedComplexes = userBookings
            .Select(b => b.Field.Complex)
            .DistinctBy(c => c.Id)
            .ToList();
        
        var userVector = CreateUserVectorWithoutLocation(bookedComplexes);

        // Lấy tất cả Complex chưa từng đặt
        var bookedComplexIds = bookedComplexes.Select(c => c.Id).ToHashSet();
        
        var allCandidates = (await _complexRepository.GetAllActiveComplexesWithDetailsAsync(null))
            .Where(c => !bookedComplexIds.Contains(c.Id))
            .ToList();

        if (!allCandidates.Any())
        {
            return new RecommendationResponse
            {
                RecommendationType = "content-based-user",
                Complexes = new List<ComplexRecommendationDto>(),
                Message = "Không có cụm sân mới để gợi ý"
            };
        }

        // 3-TIER LOCATION PRIORITY SYSTEM
        var tier1SameWard = new List<ComplexEntity>();
        var tier2SameProvince = new List<ComplexEntity>();
        var tier3OtherProvince = new List<ComplexEntity>();

        if (!string.IsNullOrEmpty(province))
        {
            foreach (var complex in allCandidates)
            {
                if (complex.Province == province)
                {
                    // Same province - check ward
                    if (!string.IsNullOrEmpty(ward) && complex.Ward == ward)
                    {
                        tier1SameWard.Add(complex); // TIER 1: Same ward (100% score)
                    }
                    else
                    {
                        tier2SameProvince.Add(complex); // TIER 2: Same province, diff ward (85% score)
                    }
                }
                else
                {
                    tier3OtherProvince.Add(complex); // TIER 3: Other province (60% score)
                }
            }
        }
        else
        {
            // No location filter - treat all as tier 1
            tier1SameWard = allCandidates;
        }

        var random = new Random();
        var allRecommendations = new List<(ComplexEntity complex, double score, int tier)>();

        // TIER 1: Same ward - NO penalty (100%)
        if (tier1SameWard.Any())
        {
            var tier1Recs = tier1SameWard
                .Select(complex => 
                {
                    var complexVector = VectorizeComplexWithoutLocation(complex);
                    var similarity = CosineSimilarity(userVector, complexVector);
                    return (complex, score: similarity, tier: 1);
                })
                .ToList();
            allRecommendations.AddRange(tier1Recs);
        }

        // TIER 2: Same province, different ward - 15% penalty (85%)
        if (tier2SameProvince.Any())
        {
            var tier2Recs = tier2SameProvince
                .Select(complex => 
                {
                    var complexVector = VectorizeComplexWithoutLocation(complex);
                    var similarity = CosineSimilarity(userVector, complexVector);
                    return (complex, score: similarity * 0.85, tier: 2);
                })
                .ToList();
            allRecommendations.AddRange(tier2Recs);
        }

        // TIER 3: Other province - 40% penalty (60%)
        if (tier3OtherProvince.Any())
        {
            var tier3Recs = tier3OtherProvince
                .Select(complex => 
                {
                    var complexVector = VectorizeComplexWithoutLocation(complex);
                    var similarity = CosineSimilarity(userVector, complexVector);
                    return (complex, score: similarity * 0.60, tier: 3);
                })
                .ToList();
            allRecommendations.AddRange(tier3Recs);
        }

        // Sort by score (with tier priority already built-in), then random for ties
        var recommendations = allRecommendations
            .OrderByDescending(x => x.score)
            .ThenBy(x => random.Next())
            .Take(topK)
            .Select(x => MapToRecommendationDto(x.complex, x.score))
            .ToList();

        return new RecommendationResponse
        {
            RecommendationType = "content-based-user",
            Complexes = recommendations,
            Message = $"Gợi ý {recommendations.Count} cụm sân phù hợp với bạn"
        };
    }

    #endregion

    #region Smart Recommendation

    /// <summary>
    /// STRATEGY 4: Smart Recommendation
    /// Tự động chọn strategy tốt nhất dựa trên context
    /// </summary>
    public async Task<RecommendationResponse> GetSmartRecommendationsAsync(
        int? userId, 
        string? province, 
        string? ward, 
        int topK = 10)
    {
        // Nếu có userId → thử personalized (with 3-tier location priority)
        if (userId.HasValue)
        {
            var personalizedResult = await GetPersonalizedRecommendationsAsync(userId.Value, topK, province, ward);
            
            if (personalizedResult.Complexes.Any())
            {
                return personalizedResult;
            }
        }

        // Fallback: location-based
        return await GetRecommendationsForNewUserAsync(province, ward, topK);
    }

    #endregion

    #region Helper Methods - Vector Operations

    /// <summary>
    /// Vector hóa Complex thành feature vector (9 features - KHÔNG BAO GỒM PROVINCE)
    /// Dùng cho Personalized recommendation để tránh bias location từ booking history
    /// Vector = [has_field_5, has_field_7, has_field_11, has_natural, has_artificial, 
    ///          avg_min_price, avg_max_price, total_bookings, avg_rating]
    /// </summary>
    private double[] VectorizeComplexWithoutLocation(ComplexEntity complex)
    {
        var activeFields = complex.Fields.Where(f => f.IsActive && !f.IsDeleted).ToList();

        // Feature 1-3: Field types available (one-hot)
        var hasField5 = activeFields.Any(f => f.FieldSize == "5") ? 1.0 : 0.0;
        var hasField7 = activeFields.Any(f => f.FieldSize == "7") ? 1.0 : 0.0;
        var hasField11 = activeFields.Any(f => f.FieldSize == "11") ? 1.0 : 0.0;

        // Feature 4-5: Surface types (one-hot)
        var hasNatural = activeFields.Any(f => f.SurfaceType?.ToLower().Contains("tự nhiên") == true) ? 1.0 : 0.0;
        var hasArtificial = activeFields.Any(f => f.SurfaceType?.ToLower().Contains("nhân tạo") == true) ? 1.0 : 0.0;

        // Feature 6-7: Price range
        var allPrices = activeFields
            .SelectMany(f => f.TimeSlots.Select(ts => ts.Price))
            .Where(p => p > 0)
            .ToList();

        var avgMinPrice = allPrices.Any() ? (double)allPrices.Min() : 0;
        var avgMaxPrice = allPrices.Any() ? (double)allPrices.Max() : 0;
        var normalizedMinPrice = Math.Min(avgMinPrice / 1000000, 1.0); // Max 1M
        var normalizedMaxPrice = Math.Min(avgMaxPrice / 1000000, 1.0);

        // Feature 8: Total bookings (popularity)
        var totalBookings = activeFields
            .SelectMany(f => f.Bookings)
            .Count(b => b.BookingStatus == BookingStatus.Completed);
        var normalizedBookings = Math.Min(totalBookings / 100.0, 1.0); // Scale to 100

        // Feature 9: Average rating
        var avgRating = GetAverageRating(complex.Id);
        var normalizedRating = avgRating / 5.0;

        return new double[] 
        { 
            hasField5,
            hasField7,
            hasField11,
            hasNatural,
            hasArtificial,
            normalizedMinPrice,
            normalizedMaxPrice,
            normalizedBookings,
            normalizedRating
        };
    }

    /// <summary>
    /// Vector hóa Complex thành feature vector (10 features - CÓ PROVINCE)
    /// Dùng cho Similar Complex recommendation
    /// Vector = [has_field_5, has_field_7, has_field_11, has_natural, has_artificial, 
    ///          avg_min_price, avg_max_price, total_bookings, avg_rating, province_code]
    /// </summary>
    private double[] VectorizeComplex(ComplexEntity complex)
    {
        var activeFields = complex.Fields.Where(f => f.IsActive && !f.IsDeleted).ToList();

        // Feature 1-3: Field types available (one-hot)
        var hasField5 = activeFields.Any(f => f.FieldSize == "5") ? 1.0 : 0.0;
        var hasField7 = activeFields.Any(f => f.FieldSize == "7") ? 1.0 : 0.0;
        var hasField11 = activeFields.Any(f => f.FieldSize == "11") ? 1.0 : 0.0;

        // Feature 4-5: Surface types (one-hot)
        var hasNatural = activeFields.Any(f => f.SurfaceType?.ToLower().Contains("tự nhiên") == true) ? 1.0 : 0.0;
        var hasArtificial = activeFields.Any(f => f.SurfaceType?.ToLower().Contains("nhân tạo") == true) ? 1.0 : 0.0;

        // Feature 6-7: Price range
        var allPrices = activeFields
            .SelectMany(f => f.TimeSlots.Select(ts => ts.Price))
            .Where(p => p > 0)
            .ToList();

        var avgMinPrice = allPrices.Any() ? (double)allPrices.Min() : 0;
        var avgMaxPrice = allPrices.Any() ? (double)allPrices.Max() : 0;
        var normalizedMinPrice = Math.Min(avgMinPrice / 1000000, 1.0); // Max 1M
        var normalizedMaxPrice = Math.Min(avgMaxPrice / 1000000, 1.0);

        // Feature 8: Total bookings (popularity)
        var totalBookings = activeFields
            .SelectMany(f => f.Bookings)
            .Count(b => b.BookingStatus == BookingStatus.Completed);
        var normalizedBookings = Math.Min(totalBookings / 100.0, 1.0); // Scale to 100

        // Feature 9: Average rating
        var avgRating = GetAverageRating(complex.Id);
        var normalizedRating = avgRating / 5.0;

        // Feature 10: Province (for location similarity)
        var provinceCode = HashProvince(complex.Province);

        return new double[] 
        { 
            hasField5,
            hasField7,
            hasField11,
            hasNatural,
            hasArtificial,
            normalizedMinPrice,
            normalizedMaxPrice,
            normalizedBookings,
            normalizedRating,
            provinceCode
        };
    }

    /// <summary>
    /// Tạo user vector từ trung bình các Complex đã đặt (KHÔNG có province)
    /// Dùng cho Personalized recommendation
    /// </summary>
    private double[] CreateUserVectorWithoutLocation(List<ComplexEntity> bookedComplexes)
    {
        if (!bookedComplexes.Any())
            return new double[9]; // 9 features without province

        var vectors = bookedComplexes.Select(VectorizeComplexWithoutLocation).ToList();
        var dimension = vectors[0].Length;
        var userVector = new double[dimension];

        for (int i = 0; i < dimension; i++)
        {
            userVector[i] = vectors.Average(v => v[i]);
        }

        return userVector;
    }

    /// <summary>
    /// Tạo user vector từ trung bình các Complex đã đặt (CÓ province)
    /// Dùng cho Similar Complex recommendation
    /// </summary>
    private double[] CreateUserVector(List<ComplexEntity> bookedComplexes)
    {
        if (!bookedComplexes.Any())
            return new double[10]; // Match vector dimension

        var vectors = bookedComplexes.Select(VectorizeComplex).ToList();
        var dimension = vectors[0].Length;
        var userVector = new double[dimension];

        for (int i = 0; i < dimension; i++)
        {
            userVector[i] = vectors.Average(v => v[i]);
        }

        return userVector;
    }

    /// <summary>
    /// Tính Cosine Similarity giữa 2 vector
    /// </summary>
    private double CosineSimilarity(double[] vectorA, double[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            return 0;

        var dotProduct = 0.0;
        var magnitudeA = 0.0;
        var magnitudeB = 0.0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }

    /// <summary>
    /// Hash province name to [0, 1]
    /// </summary>
    private double HashProvince(string? province)
    {
        if (string.IsNullOrEmpty(province))
            return 0.0;

        return (province.GetHashCode() % 100) / 100.0;
    }

    #endregion

    #region Helper Methods - Data Mapping

    private ComplexRecommendationDto MapToRecommendationDto(ComplexEntity complex, double score)
    {
        var activeFields = complex.Fields.Where(f => f.IsActive && !f.IsDeleted).ToList();
        
        // Price range
        var allPrices = activeFields
            .SelectMany(f => f.TimeSlots.Select(ts => ts.Price))
            .Where(p => p > 0)
            .ToList();
        
        var priceRange = allPrices.Any() 
            ? $"{FormatPrice(allPrices.Min())} - {FormatPrice(allPrices.Max())}"
            : "Chưa cập nhật";

        // Field types
        var fieldTypes = activeFields
            .Select(f => $"Sân {f.FieldSize}")
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        // Surface types
        var surfaceTypes = activeFields
            .Select(f => f.SurfaceType)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();

        // Total bookings
        var totalBookings = activeFields
            .SelectMany(f => f.Bookings)
            .Count(b => b.BookingStatus == BookingStatus.Completed);

        var avgRating = GetAverageRating(complex.Id);
        var imageUrl = complex.ComplexImages?.FirstOrDefault()?.ImageUrl;

        return new ComplexRecommendationDto
        {
            Id = complex.Id,
            Name = complex.Name,
            Province = complex.Province,
            Ward = complex.Ward,
            Street = complex.Street,
            Phone = complex.Phone,
            PriceRange = priceRange,
            FieldTypes = fieldTypes,
            SurfaceTypes = surfaceTypes!,
            TotalFields = activeFields.Count,
            AverageRating = avgRating,
            TotalBookings = totalBookings,
            SimilarityScore = Math.Round(score, 3),
            ImageUrl = imageUrl,
            IsActive = complex.IsActive,
            OpeningTime = complex.OpeningTime?.ToString(@"hh\:mm"),
            ClosingTime = complex.ClosingTime?.ToString(@"hh\:mm")
        };
    }

    private double GetAverageRating(int complexId)
    {
        var reviews = _reviewRepository.GetAllAsync(r => 
            !r.IsDeleted && 
            r.Booking.Field.ComplexId == complexId
        ).Result;
        
        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    private string FormatPrice(decimal price)
    {
        if (price >= 1000000)
            return $"{price / 1000000:0.#}tr";
        if (price >= 1000)
            return $"{price / 1000:0}k";
        return $"{price:0}đ";
    }

    #endregion
}
