# Test Recommendation API Endpoints

# Set your API base URL
$baseUrl = "http://localhost:5033/api/recommendations"

Write-Host "🧪 Testing Recommendation System v2.0 (Complex-level)" -ForegroundColor Cyan
Write-Host ""

# Test 1: Similar Complexes
Write-Host "Test 1: Similar Complexes" -ForegroundColor Yellow
Write-Host "Endpoint: GET $baseUrl/similar-complex/1?topK=5" -ForegroundColor Gray
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/similar-complex/1?topK=5" -Method Get
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Type: $($response.data.recommendationType)" -ForegroundColor Gray
    Write-Host "Found: $($response.data.complexes.Count) complexes" -ForegroundColor Gray
    if ($response.data.complexes.Count -gt 0) {
        $first = $response.data.complexes[0]
        Write-Host "First result: $($first.name) - Score: $($first.similarityScore)" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: New User
Write-Host "Test 2: New User (Location-based)" -ForegroundColor Yellow
Write-Host "Endpoint: GET $baseUrl/new-user?province=Hồ Chí Minh&topK=5" -ForegroundColor Gray
try {
    $encodedProvince = [System.Web.HttpUtility]::UrlEncode("Hồ Chí Minh")
    $response = Invoke-RestMethod -Uri "$baseUrl/new-user?province=$encodedProvince&topK=5" -Method Get
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Type: $($response.data.recommendationType)" -ForegroundColor Gray
    Write-Host "Found: $($response.data.complexes.Count) complexes" -ForegroundColor Gray
    if ($response.data.complexes.Count -gt 0) {
        $first = $response.data.complexes[0]
        Write-Host "First result: $($first.name) - Rating: $($first.averageRating)" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Personalized (requires JWT token)
Write-Host "Test 3: Personalized (Content-based)" -ForegroundColor Yellow
Write-Host "⚠️  Skipped - Requires JWT token" -ForegroundColor Yellow
Write-Host "Manual test: Set `$token variable and uncomment code" -ForegroundColor Gray
<#
$token = "YOUR_JWT_TOKEN_HERE"
$headers = @{
    "Authorization" = "Bearer $token"
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/personalized?topK=5" -Method Get -Headers $headers
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Type: $($response.data.recommendationType)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
#>
Write-Host ""

# Test 4: Smart Recommendation
Write-Host "Test 4: Smart Recommendation" -ForegroundColor Yellow
Write-Host "Endpoint: GET $baseUrl/smart?province=Hồ Chí Minh&topK=5" -ForegroundColor Gray
try {
    $encodedProvince = [System.Web.HttpUtility]::UrlEncode("Hồ Chí Minh")
    $response = Invoke-RestMethod -Uri "$baseUrl/smart?province=$encodedProvince&topK=5" -Method Get
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Type: $($response.data.recommendationType)" -ForegroundColor Gray
    Write-Host "Found: $($response.data.complexes.Count) complexes" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "🎉 Testing complete!" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Notes:" -ForegroundColor White
Write-Host "- Make sure your API is running on $baseUrl" -ForegroundColor Gray
Write-Host "- Test 3 requires valid JWT token" -ForegroundColor Gray
Write-Host "- Response structure: { success, message, data: { recommendationType, complexes[] } }" -ForegroundColor Gray
