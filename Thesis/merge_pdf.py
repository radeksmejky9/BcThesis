import pikepdf

# Otevři soubory
prace = pikepdf.open("ki-thesis.pdf")
zadani = pikepdf.open("zadani.pdf")

# Vytvoř kopii práce jako základ (zachová všechny odkazy a metadata)
output = pikepdf.open("ki-thesis.pdf")

# Smaž stránky 3 a 4 (kde je nepodepsané zadání)
del output.pages[2:4]

# Vlož podepsané zadání na pozici 2 (po titulce a druhé stránce)
for i, page in enumerate(zadani.pages[0:2]):
    output.pages.insert(2 + i, page)

# Ulož
output.save("../zaverecna-prace.pdf")

print("Hotovo! Odkazy zachovány.")
