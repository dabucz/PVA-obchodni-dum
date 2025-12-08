# Software Requirements Specification (SRS)

**Název projektu:** Simulační model front v obchodním domě

**Datum:** 18. listopadu 2025

**Autoři:** Jan Boháček, Daniel Kryhut

**Verze: 1.1.0**

---

## 1. Úvod

### 1.1 Cíl dokumentu

Cílem tohoto dokumentu je definovat kompletní soubor požadavků na software pro simulaci front v obchodním domě. Systém bude modelovat chování zákazníků, pokladen a front s cílem analyzovat dobu čekání a optimalizovat počet otevřených pokladen.

### 1.2 Rozsah projektu

Projekt vytvoří aplikaci umožňující provádět diskrétní událostní simulace provozu pokladen. Výsledky simulace budou sloužit pro rozhodování o strategii řízení počtu otevřených pokladen.
Rozsah zahrnuje:

* Modelování příchodů zákazníků a jejich obsluhy.
* Měření klíčových metrik (doba čekání, délka fronty, vytížení pokladen).
* Vizualizaci výsledků v konzoli.
* Možnost měnit vstupní parametry (počet pokladen).

### 1.3 Definice a zkratky

* CLI – Command Line Interface (konzolové rozhraní)

### 1.4 Reference na další dokumenty

* Funkční specifikace (FS): Simulační model front v obchodním domě

---

## 2. Popis systému

### 2.1 Přehled systému

Systém simuluje provoz pokladen v obchodním domě. Zákazníci přicházejí v náhodných časových intervalech, mají náhodný počet položek a jsou obsluhováni na pokladnách, jejichž počet je možné měnit. Výsledky jsou prezentovány v textové podobě.

### 2.2 Funkční a nefunkční požadavky

Systém bude mít následující funkční a nefunkční požadavky.

#### Funkční požadavky (viz detailní kapitola 3)

* Simulace příchodů a odchodů zákazníků.
* Nastavení parametrů simulace (počet pokladen).
* Výpočet a zobrazení.
* Export výsledků.

#### Nefunkční požadavky (viz detailní kapitola 4)

* Rychlý běh i při velkém počtu zákazníků.
* Přehledný výstup.
* Udržovatelnost a jednoduché rozšiřování.

### 2.3 Omezení a předpoklady

* Systém bude spuštěn v textovém režimu, bez grafického rozhraní.
* Počet zákazníků i pokladen je omezen pamětí a výkonem počítače.
* Systém nepoužívá reálná data – generuje je podle zadaných rozdělení.
* Předpokládá se běh na standardním PC s možností spuštění cs souborů.

---

## 3. Funkční požadavky

### 3.1 Popis funkcí

#### 3.1.1 Spuštění simulace

**Popis:** Uživatel zadá parametry simulace.

**Vstupy:** Parametry simulace (počet otevřených pokladen).

**Výstupy:** Shrnutí nastavení a potvrzení spuštění simulace.

**Chování:** Systém inicializuje události a spustí hlavní smyčku.

#### 3.1.2 Generování zákazníků                                         

**Popis:** Systém generuje zákazníky podle zvoleného rozdělení příchodů.

**Vstupy:** Počet zákazníku za stanovený čas.

**Výstupy:** Události příchodu zákazníků.

**Chování:** Zákazníci jsou postupně přidáváni do front.

#### 3.1.3 Obsluha na pokladně

**Popis:** Každý zákazník je přiřazen k pokladně, kde čeká, dokud není obsloužen.

**Vstupy:** Počet položek.

**Výstupy:** Čas dokončení obsluhy, uvolnění pokladny.

**Chování:** Po ukončení obsluhy se spouští další zákazník z fronty.

#### 3.1.4 Výpočet statistik

**Popis:** Během simulace se průběžně ukládají metriky o čekání, frontách a vytížení.

**Vstupy:** Časové události.

**Výstupy:** Souhrnné statistiky po skončení simulace.

**Chování:** Výsledky se zprůměrují a zobrazí na konci.

#### 3.1.5 Konzolová vizualizace

**Popis:** Uživatel vidí průběžné i finální výsledky simulace přímo v konzoli.

**Vstupy:** Výsledné statistiky.

**Výstupy:** Textový přehled.

**Chování:** Po dokončení simulace se vypíše shrnutí, např. průměrná doba čekání a vytížení pokladen.

---

## 4. Nefunkční požadavky

### 4.1 Výkonnost

* Simulace musí být schopna zpracovat minimálně 10 000 zákazníků během jedné simulace do 5 sekund.
* Měření metrik musí probíhat bez významného zpomalení běhu.

### 4.2 Spolehlivost

* Systém musí poskytovat reprodukovatelné výsledky při stejném seed generátoru náhodných čísel.
* Musí detekovat chybné vstupy a vypisovat chybové zprávy.

### 4.3 Bezpečnost

* Aplikace neběží s privilegovanými oprávněními.
* Nezpracovává citlivá data.

### 4.4 Udržovatelnost

* Kód musí být modulární a komentovaný.
* Parametry simulace budou načítány z CLI argumentů.

---

## 5. Požadavky na hardware a software

* Hardware: Standardní PC (min. 2 GB RAM, 2 GHz CPU)
* Software:
    * Operační systém: Windows / Linux / macOS
    * C# 9.0 nebo vyšší

## 6. Přílohy

---

### 6.1 Diagram systému

[ Zákazníci ] --> [ Fronty pokladen ] --> [ Pokladny ] --> [ Statistiky ] --> [ Vizualizace ]

### 6.2 Příklad obrazovky (konzolový výstup)
Simulace dokončena.
Počet zákazníků: 480
Počet pokladen: 20
Průměrná doba čekání: 3.2 min
Průměrná délka fronty: 2.1
Vytížení pokladen: 70%

### 6.3 Reference na FS

Tento dokument SRS úzce navazuje na funkční specifikaci (FS) projektu a slouží jako výchozí podklad pro implementaci simulace front.

*Konec dokumentu.*
