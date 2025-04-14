projekt generic blockchain

realizowane przez studentów UWr we współpracy z Nokią 

uczestnicy: Kacper Osadowski, Marceli Buczek, Taras Tsehenko, Yelyzaveta Ilman 

wymagane jest środowisko dotnet w wersji 9.0.X

## włączanie węzła

git clone https://github.com/Dalinar-Kholin/super-generic-blockchain

cd blockProject/TestProject

dotnet run -- \<port na którym ma działać server http węzła>

cała interakcja z blockchainem odbywa się poprzez server http na localhost:port podanym w argv, server tcp będzie działać na porcie http+1

dodanie pierwszego węzła odbywa się przez zapytanie http o sygnaturze

http://127.0.0.1: \<port servera>/api/addNewNode?port=<port servera tcp węzła który chcemy dodać>&ip=<ip węzła który chcemy dodać>"


## testowanie
testy znajdują się w projekcie testowym TestProject

wykonujemy


git clone https://github.com/Dalinar-Kholin/super-generic-blockchain

cd blockProject/TestProject

dotnet test




