cd C:\Users\Jack\source\repos\KalendarzProjektPW\KalendarzProjektPW\bin\Debug\Rozklad
timeout /t 1
del .\old.txt
timeout /t 1
move .\new.txt .\old.txt
timeout /t 1
move .\Down\*.txt .\new.txt
timeout /t 1
