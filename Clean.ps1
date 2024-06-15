# Get the current directory from where the script is executed
$startPath = Get-Location

# List of directory names to search and delete
$directoriesToDelete = @("bin", "obj", ".vs")

# Find and remove the directories recursively
foreach ($dir in $directoriesToDelete) {
    Get-ChildItem -Path $startPath -Recurse -Directory -Force -Name $dir |
    ForEach-Object { 
        $fullPath = Join-Path -Path $startPath -ChildPath $_
        Write-Host "Deleting $fullPath" -ForegroundColor Red
        Remove-Item -Path $fullPath -Recurse -Force
    }
}

Write-Host "Cleanup completed." -ForegroundColor Green
