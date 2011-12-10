#Run Redis as Service on Windows#
[Redis](http://redis.io/) is one of the fastest and most feature-rich in-memory key value data stores to come from the NoSQL movement.

If you run Redis on Windows machine, the best option is to run each Redis instance as Windows service. This project helps you to do that. It was tested with [Redis binaries](https://github.com/dmajkic/redis/downloads).

[Compiled executable](https://github.com/kcherenkov/redis-windows-service/downloads).

##Install service##
To install Redis as service, just compile project sources, **put it in the same folder with "redis-server" and "redis-cli" executables** and then execute:

    sc create %name% binpath= "\"%binpath%\" %configpath%" start= "auto" DisplayName= "Redis"

Where:  
*%name%* -- name of service instance, ex. `redis-instance`;  
*%binpath%* -- path to this project exe file, ex. `C:\Program Files\redis\RedisService.exe`;  
*%configpath%* -- path to redis configuration file, ex. `E:\Redis\redis.conf`;  
To understand another properties, like `start` and `DisplayName`, read article [How to create a Windows service by using Sc.exe](http://support.microsoft.com/kb/251192).

##Uninstall service##
To uninstall service, simply execute:

	sc delete %name%

Where *%name%* -- name of already installed service instance.

##Run from console##
For testing purposes, you can run redis service in console mode, like

	%binpath% %configpath%
