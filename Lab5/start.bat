@echo off
set /a phil_count=10

start Lab5\bin\Debug\Lab5.exe lunch %phil_count%
for /l %%A in (1, 1, %phil_count%) do (
	start Lab5\bin\Debug\Lab5.exe phil
)

pause

taskkill /im dc4.exe /f