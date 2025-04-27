plan jest taki że
klucz publiczny może byc tożsamy z kontem
samo zakładanie konta może wyglądać tak że nowy klucz publiczny wysyła wiadomość do wszystkich informującą o tym kim jest \


rekord danych składa się z tego \
klucz publiczny tego kto to wysłał \
klucz publiczny tego do kogo wysyłamy \
klucz AES potrzemny do szyfrowania wiadomości \
IV aes vektor inicjujący \ 
sama wiadomość zaszyfrowana/lub nie \
czy wiadomość jest zaszyfrowana \

jako kluczy używamy ECDH \
jako algorytmu szyfrującego uzywamy AES w trybie CBC \

za pomocą kluczy asymetrycznych szyfrujemy tylko i wyłącznie klucz AES oraz IV \


jeżeli wysyłamy wiadomość do wszystkich adresatem jest adres 0x0
