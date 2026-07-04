# Script pour les migrations Entity Framework
Write-Host "=== Gestion des migrations EF Core ===" -ForegroundColor Cyan
Write-Host ""

$action = Read-Host "Choisissez une action (add/update/remove)?"

switch ($action) {
    "add" {
        $name = Read-Host "Nom de la migration"
        dotnet ef migrations add $name --context ApplicationDbContext
    }
    "update" {
        dotnet ef database update --context ApplicationDbContext
    }
    "remove" {
        dotnet ef migrations remove --context ApplicationDbContext
    }
    default {
        Write-Host "Action non reconnue" -ForegroundColor Red
    }
}