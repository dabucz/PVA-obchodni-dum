#Funkční Specifikace (FS): Simulační model front v obchodním domě

**Datum:** 9. prosince 2025
**Autoři:** Jan Boháček, Daniel Kryhut
**Verze:** 1.1.1
**Reference na SRS:** SRS Simulační model front v obchodním domě v. 1.1.0

-----

## 1\. Účel

Tento dokument podrobně popisuje, jak budou implementovány funkční požadavky definované v dokumentu **Software Requirements Specification (SRS)** pro simulační model front v obchodním domě. Zaměřuje se na implementační detaily, návrh uživatelského rozhraní (konzolového), vstupy, výstupy, chybové stavy a datové formáty.

-----

## 2\. Návrh Uživatelského Rozhraní (Konzole)

Vzhledem k požadavku na **CLI (Command Line Interface)** bude interakce s uživatelem probíhat výhradně v textové konzoli.

### 2.1 Spuštění a Nastavení Parametrů

Program se spustí s parametry z příkazové řádky (CLI argumenty).

**Navigace:**

1.  Spuštění programu: `dotnet run`
2.  Pokud argumenty chybí nebo jsou neplatné, zobrazí upozornění pro zadání nových argumentů.

**Formát Vstupů (CLI Argumenty):**

| Argument | Popis | Typ | Omezení | Příklad |
| :--- | :--- | :--- | :--- | :--- |
| `počet_pokladen` | Počet otevřených pokladen. | `int` | $\ge 1$ | 5 |
| `doba_simulace_minuty` | Celková doba běhu simulace v minutách. | `int` | $\ge 1$ | 60 |
| `průměrný_interval_příchozích_sekundy` | Průměrný interval (sekundy) mezi příchody zákazníků (pro Poissonovo rozdělení). | `double` | $> 0$ | 30.0 |
| `průměrný_počet_položek` | Průměrný počet položek v košíku (pro Gaussovo/Poissonovo rozdělení). | `double` | $> 0$ | 15.0 |

**Příklad spuštění:**
`dotnet run`

### 2.2 Vizualizace Průběhu a Finální Výstup

Vizualizace proběhne ve dvou fázích: průběžná (volitelně) a finální shrnutí.

#### Průběžná Vizualizace (Volitelná pro odladění)

Zobrazení klíčových událostí v reálném čase, např.:
`[Čas: 10:35] Přišel zákazník #15 (20 položek). Zařadil se do fronty u Pokladny 3 (aktuální délka: 1).`

#### Finální Konzole Výstup (Požadované)

Po dokončení simulace se zobrazí ucelený přehled statistik.

**Formát Výstupů:**
Výstup bude formátovaný s jasnými popisky, s desetinnými čísly zaokrouhlenými na 1-2 místa

| Metrika | Formát | Příklad |
| :--- | :--- | :--- |
| **Simulace dokončena.** | Textový hlavička | **Simulace dokončena.** |
| Počet zákazníků | `int` | Počet zákazníků: 480 |
| Počet pokladen | `int` | Počet pokladen: 4 |
| Průměrná doba čekání | `double` (min.) | Průměrná doba čekání: **3.2 min** |
| Průměrná délka fronty | `double` | Průměrná délka fronty: **2.1** |
| Maximální délka fronty | `int` | Maximální délka fronty: 8 |
| Vytížení pokladen | `double` (%) | Vytížení pokladen: **70.5%** |
| Doba trvání simulace | `double` (sek.) | Doba běhu simulace: 4.12 sek. |

-----

## 3\. Realizace Funkčních Požadavků

### 3.1 Spuštění simulace

  * **Implementace:** Hlavní metoda `Main`.
  * **Chování:** Vytvoří se instance třídy **`SimulationManager`** (viz 5.1.3) a zavolá se její metoda `RunSimulation()`.

### 3.2 Generování zákazníků

  * **Modelování příchodů:** Intervaly mezi příchody zákazníků budou generovány pomocí **Exponenciálního rozdělení**, s definovaným průměrem.
  * **Modelování položek:** Počet položek v košíku bude náhodně generován.

### 3.3 Obsluha na pokladně

  * **Doba obsluhy:** Předpokládá se, že doba obsluhy je **přímo úměrná počtu položek** plus fixní čas (např. platba).
      * **Vzorec:** $T_{obsluhy} = T_{fixní} + N_{položek} \times T_{za\_položku}$
  * **Výběr pokladny:** Zákazník si vybírá pokladnu s **nejkratší frontou** (FIFO - First-In, First-Out).

### 3.4 Výpočet statistik

  * **Metriky (během simulace):**
      * **Doba čekání:** Čas, kdy zákazník vstoupil do fronty, odečtený od času, kdy začal být obsluhován.
      * **Délka fronty:** Počet zákazníků ve frontě v klíčových časových bodech.
      * **Vytížení pokladny:** Celkový čas, kdy byla pokladna obsazena, dělený celkovou dobou simulace.
  * **Souhrnné statistiky (po simulaci):** Vypočítá se průměr z uložených individuálních dob čekání a délek front.

-----

## 4\. Chybové stavy a Zotavení z chyb

| Chybový stav | Popis | Chybová zpráva (Konzole) | Zotavení/Akce |
| :--- | :--- | :--- | :--- |
| **Neplatné argumenty** | Chybí nebo jsou nečíselné/mimo rozsah. | `Chyba: Neplatné argumenty. Zadejte kladná celá čísla. Použití: dotnet run <pokladny> <doba_simulace>...` | Program vypíše pro zadání nového argumentu. |
| **Přetečení/Výkon** | Simulace trvá déle než stanovený limit (5s). | `Upozornění: Simulace trvala X sekund (překročena hranice 5s). Zvažte zkrácení doby simulace.` | Simulace se dokončí, ale vypíše se upozornění. |
| **Interní chyba** | Neočekávaná výjimka. | `Kritická chyba: V simulaci došlo k neočekávané chybě: [Popis chyby].` | Program se ukončí. |

-----

## 5\. Datové formáty a Uložení

Simulace je navržena pro **diskrétní událostní simulaci**. Data budou udržována v paměti po dobu běhu programu.

### 5.1 Datové Struktury (Návrh C\# Tříd)

#### 5.1.1 Třída `Customer` (Zákazník)

  * `Id`: `int`
  * `Items`: `int`
  * `ArrivalTime`: `double` (čas příchodu, v sekundách od startu)
  * `QueueEntryTime`: `double`
  * `ServiceStartTime`: `double`
  * `ServiceEndTime`: `double`

#### 5.1.2 Třída `Checkout` (Pokladna)

  * `Id`: `int`
  * `IsBusy`: `bool`
  * `Queue`: `Queue<Customer>` (Fronta zákazníků – FIFO)
  * `CurrentCustomer`: `Customer`
  * `TotalBusyTime`: `double` (pro výpočet vytížení)

#### 5.1.3 Třída `SimulationManager` (Řídící mechanismus)

  * `RandomGenerator`: `Random`
  * `CurrentTime`: `double` (globální čas simulace)
  * `Checkouts`: `List<Checkout>`
  * `AllCustomers`: `List<Customer>`
  * `EventList`: `SortedList<double, Action>` (Seznam budoucích událostí)

-----