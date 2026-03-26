# Build et pousse les images vers Docker Hub unterbergerbruno
param(
    [string]$Tag = "latest"
)

$ErrorActionPreference = "Stop"
$Registry = "unterbergerbruno"

Write-Host "==> Build $Registry/partage-texte-api:$Tag" -ForegroundColor Cyan
docker build -f src/PartageTexte.Api/Dockerfile -t "$Registry/partage-texte-api:$Tag" .

Write-Host "==> Build $Registry/partage-texte-web:$Tag" -ForegroundColor Cyan
docker build -f src/PartageTexte.Web/Dockerfile -t "$Registry/partage-texte-web:$Tag" .

Write-Host "==> Push" -ForegroundColor Cyan
docker push "$Registry/partage-texte-api:$Tag"
docker push "$Registry/partage-texte-web:$Tag"

Write-Host "==> Done" -ForegroundColor Green
Write-Host "    $Registry/partage-texte-api:$Tag"
Write-Host "    $Registry/partage-texte-web:$Tag"
