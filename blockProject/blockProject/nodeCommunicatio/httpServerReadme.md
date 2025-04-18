plik opisujący dokumentacje servera http oraz ogólną zasadę jego działania

### cel:
server http służy TYLKO I WYŁĄCZNIE w celu interakcji uzytkownika z serverem wezła sieci blockchain


### ogólne zasady
serwer zawsze zwraca plik json o sygnaturze
{\
    success: bool, // true w razie sukcesu wywołania \
    object: object // informacja dodana do reszultatu mogą to być dane w razie success true, albo dane błędu w razie success false \
}

api:

### get Stat
method: GET\
pobieranie podstawowych statystyk węzła\
`http://<ip>:<port>/api/getStats` \
w razie sukcesu\
result = `{"blockCount":5,"recordCount":6,"workingTime":304,"friendNodeCount":1,"friendNode":["127.0.0.1:8080"]}`
ta funkcja chyba nie może się źle wykonać -- prawo marshala proszę nie zadziałaj\
gdzie blockCount: int - ile jest bloków w blockchaine, recordCount: int - liczba dyskretnych ramek danych, workingTime: int64 - czas działania servera w sekundach, friendNodeCount: int - liczba znajomych węzłów, friendNode: []string - tablica stringów w postaci ip:port\


### addNode
method: GET\
dodawanie węzła

argumenty przekazywane poprzez querry

ip = ipservera w postaci X.X.X.X \
port = port na którym działa server tcp węzła

`http://<ip>:<port>/api/addNode?ip=<fip>&port=<fport>`

w razie sukcesu zwraca `{success = true, result = null}`
w razie błędu zwraca `{success = false, result = "<errorMessage>"}`


### addRecord
method: POST\
dodawanie rekordu danych do blockchainu\
dane przekazywane poprzez body\
sygnatura JSON = `{ Key : <keyvalue>, Value: <valueValue/*XD*/>}`\
w razie sukcesu zwraca `{success = true, result = ""}`
w razie błędu zwraca `{success = false, result = "<errorMessage>"}`
