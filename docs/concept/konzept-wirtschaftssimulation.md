# Konzeptdokument: Working Title "Öræfi" (Platzhaltername, vormals "Landnám" — siehe Abschnitt 16 zur Namenskollision)

Historisches Wirtschafts- und Freiheits-Sandbox-Spiel, angesiedelt in Island zur Landnahmezeit (ca. 874–930 n. Chr.).
Singleplayer, simulierte NPC-Ökonomie, kein Magiesystem, eigenständiges Fertigkeitssystem (kein DSA-Bezug mehr).
Zielplattform: Computerspiel, Unity (C#), isometrische Darstellung.

---

## 1. Kernprinzipien (Design Pillars)

| Prinzip | Bedeutung | Nicht-Ziel |
|---|---|---|
| Absolute Wahlfreiheit | Kein Hauptplot, Landbeanspruchung (Landnám) als Startakt | Kein linearer Questpfad |
| Wirtschaft als Hauptsystem | Angebot/Nachfrage/Produktion treiben das Spiel | Kampf ist kein Progressionskern |
| Historischer Realismus | Reale Ökologie, Handwerk, Landwirtschaft, Handelsgüter Islands/Norwegens | Keine Fantasy-Elemente, keine Magie |
| Plausible Technologie | Alles nutzbar, was mit era-typischen Materialien/Mechanik machbar gewesen wäre — auch wenn nicht historisch belegt | Keine Elektrizität, keine Präzisions-/Druckmechanik (z. B. Dampfkraft) |
| Kampf sekundär | Risiko-/Schutzmechanik für Hof und Transport | Kein Loot-Grinding, keine Klassenbalance |
| Skill-basierter Charakter | Fertigkeiten wachsen durch Nutzung (learning-by-doing), eigenständiges System | Keine Level/XP-Klassen |
| Kultur als Kulisse | Vorchristlicher Glaube (Ásatrú), Thing-Versammlungen, Sagas als Atmosphäre | Keine Gameplay-Magie, keine übernatürlichen Effekte |
| Generationenspiel | Charaktertod ist erwartbarer Teil des Spiels, Erbfolge übernimmt den Hof (Die-Gilde-Prinzip) | Kein Einzelheld-Fokus, kein Permadeath-Spielende |

⚠️ Unsicherheit: "absolute Wahlfreiheit" + tiefe Wirtschaftssimulation stehen in Spannung — je freier der Spieler, desto schwerer die Simulation balancierbar. Adressiert in Abschnitt 8.

---

## 2. Setting: Island, Landnahmezeit (ca. 874–930 n. Chr.)

**Rechtlicher Status:** Vollständig unproblematisch — reale Geschichte und Geographie sind gemeinfrei.

**Startpunkt:** Spieler beginnt als Einzelsiedler, der unbeanspruchtes Land in Besitz nimmt (Landnám-Akt).

**Politische Rahmenstruktur (optional, spätere Ausbaustufe):** Hreppur (Gemeindeverbände), Þing-Versammlungen, Alþingi-Gründung 930 als möglicher Meilenstein, Goði als NPC-Rollentyp.

**Kulturelle Kulisse:** Vorchristlicher Glaube (Blót-Opferfeste, Thing-Rituale, saga-typische Erzählkultur) rein atmosphärisch. Keine Gameplay-Mechanik.

---

## 3. Ökologie und Ressourcenrealismus

### 3.1 Vegetation bei Landnahme
Signifikanter Birkenwald-/Buschbestand (*Betula pubescens*), Niederwald, nicht bautauglich. Erschöpfbare, langsam regenerierende Ressource — Übernutzung führt zu Bodenerosion und dauerhaftem Fruchtbarkeitsverlust.

### 3.2 Fauna
Keine heimischen großen Landsäugetiere außer Polarfuchs. Jagen/Fangen bedeutet:
- **Vogelfang an Klippen** — inkl. **Papageientaucher (Puffins)**: Der Atlantische Papageientaucher brütet seit der letzten Eiszeit in Island und war zur Landnahmezeit bereits fest etabliert (große Kolonien u. a. Westmännerinseln, Látrabjarg) — historisch wie heute mit dem Kescher-Fang (háfur) von Klippen/Booten aus bejagt, saisonal an die Brutzeit (etwa Mai–August) gebunden
- Robbenjagd an der Küste
- Walstrandungen (seltenes, wertvolles Zufallsereignis)
- Fischerei (Kabeljau)

Mitgebrachtes Vieh (Schafe, Rinder, Pferde, Ziegen, Schweine, Geflügel) bildet die Nahrungsgrundlage — Weidewirtschaft und Milchverarbeitung (Skyr) sind zentrale Wirtschaftszweige.

### 3.3 Sammelressourcen
Wildbeeren, essbare Wildkräuter, Dulse/Röt-Algen, Flechten (Mangelnahrung), begrenzte Pilzvielfalt (⚠️ ggf. spielerische Freiheit vor Detailrealismus priorisieren).

---

## 4. Wirtschaftssimulation (Kernsystem)

### 4.1 Architektur: Agenten-basiert, zweistufig
Jeder Hof/jede Siedlung = eigener Wirtschaftsagent (Bedürfnishierarchie, Produktionskapazität, Lagerbestand mit Verderb, lokale Preisbildung). Zwei-Ebenen-Modell: detaillierte Simulation nahe am Spieler, abstrahierte Tick-Berechnung für entfernte Höfe.

### 4.2 Güterkette Island ↔ Norwegen
**Import:** Bauholz (kritisch), Eisen, Getreide (Gerste), Teer, Honig, Glasperlen/Schmuck (fertig importiert — lokale Glasherstellung bleibt ausgeschlossen, siehe Abschnitt 6), später Luxusgüter.
**Export:** Vaðmál (Wollstoff, zugleich Zahlungsmittel), Stockfisch, Häute, Wolle, Gerfalken (Einzelwert sehr hoch), Walross-Elfenbein.

### 4.3 Saisonalität des Seehandels
Nordatlantik-Passage praktisch nur Mai–September. Importgüter treffen schubweise ein. ⚠️ Unsicherheit: Knorr-Kapazität ca. 15–35 t, Fahrtzeit Norwegen–Island ca. 7–10 Tage bei günstigem Wind, hohe Varianz.

### 4.4 Rollenteilung Seefahrt
Küstenboote (Fischerei/Nahtransport) waren Alltagswerkzeug; Atlantiküberquerung erforderte einen spezialisierten Steuermann (stýrimaður). Schiffsanteile wurden häufig geteilt (félag).

**Spielmechanik:**
- **Standardweg:** Fracht an Hafenhandelsplatz übergeben (Verkauf/Frachtanteil bei NPC-Kapitän) — niedrige Marge, kein Risiko, kein Skill nötig
- **Ausbauweg:** Fertigkeit "Steuermannskunst" erlernen, Schiffsanteile erwerben, selbst fahren — höhere Marge, echtes Sturm-/Navigationsrisiko

---

### 4.5 Unfreie Arbeitskraft (Þrælar)

Historisch belegt als strukturell bedeutsamer Teil der Gründungsökonomie, nicht als Randphänomen: <cite index="40-1">Schätzungen zufolge machten Þrælar 10–25 % der Gründungsbevölkerung Islands aus, oft aus Raubzügen in Britannien und Irland stammend, und waren wesentlich für die Rodung von Land und die Errichtung von Höfen.</cite> ⚠️ Diese Prozentangabe stammt aus einer Quelle mit unklarer akademischer Reputation und sollte als Größenordnung, nicht als exakter Wert gelten. <cite index="39-1">Männliche Þrælar leisteten schwere körperliche Arbeit — Waldrodung, Torfstechen, Viehhaltung, Rudern — während weibliche Þrælar überwiegend häusliche Arbeit (Kochen, Melken, Wollverarbeitung) übernahmen.</cite> <cite index="42-1">Sklaverei war nach den Grágás-Gesetzen bis 1270 rechtlich erlaubt, wird aber im Gesetzestext Kristinna laga þáttur (1122–1133) letztmals als bestehende Praxis erwähnt — danach sind keine Þrælar mehr belegt.</cite>

**Spielmechanik — zwei gekoppelte Risiken:**

1. **Versorgungspflicht (Mortalitätsrisiko):** Þrælar benötigen Nahrung, Kleidung und Unterkunft wie freie Haushaltsmitglieder. Unterversorgung führt zu Krankheit und Tod — eine reale Ressourcenbindung, kein kostenloser Arbeitskraft-Bonus. Das macht Þrælar wirtschaftlich zweischneidig: mehr Arbeitskraft, aber auch mehr Verbrauchsbedarf in einer ohnehin knappen Wirtschaft.

2. **Aufstands-/Fluchtrisiko:** Historisch verankert im bekanntesten Einzelfall der gesamten Landnahme: <cite index="49-1">Hjörleifr Hróðmarsson zwang im ersten Siedlungsjahr (874/875) zehn irische Þrælar, mangels Ochsen selbst den Pflug zu ziehen; sie töteten daraufhin den einzigen Ochsen, schoben es auf einen Bären, lockten Hjörleifr zur "Bärenjagd" und erschlugen ihn und seine Männer, bevor sie mit den Frauen der Siedlung sowie Nahrung und Waffen flohen.</cite> <cite index="55-1">Ingólfr Arnarson verfolgte und tötete die geflohenen Þrælar auf den seither nach ihnen benannten Vestmannaeyjar.</cite>

Das liefert ein direktes, saga-authentisches Vorbild für eine Revolte-Mechanik: harte/erniedrigende Behandlung (Überarbeitung, Misshandlung, extreme Unterversorgung) erhöht ein Aufstands-/Fluchtrisiko, das über bloße Mortalität hinausgeht — Verlust von Ausrüstung/Vieh, im Extremfall Gefahr für den Spielercharakter selbst.

3. **Freilassung als Wirtschaftsventil (nicht nur Ausweg):** Rechtlich vorgesehener Weg (Freilassung, historisch belegt), der direkt an die "Freigelassener"-Herkunftsoption aus Abschnitt 11.1 anknüpft. Übersteigt der Versorgungsaufwand eines Þræll seinen Arbeitswert, ist Freilassung eine rationale wirtschaftliche Entscheidung, kein reiner Moralakt — und reduziert gleichzeitig den Verwaltungsdruck auf den Haushalt. Ein freigelassener Þræll kann optional als freies Gesinde (Vinnufólk, gegen Lohn statt Vollversorgung) bleiben oder den Hof verlassen; narrativ ist das zugleich die mögliche Vorgeschichte einer NPC- oder Spielerfigur.

**Steuerungsprinzip — Aufgabenzuweisung statt Mikromanagement (Frostpunk-Vorbild):** Þrælar und freies Gesinde werden nicht einzeln durch Aktionen gesteuert, sondern bekommen eine Rolle/Aufgabe zugewiesen (z. B. "Wollverarbeitung", "Fischerei", "Torfstechen") und erledigen sie **eigenständig** über Tage/Jahreszeiten hinweg, moduliert durch ihren Fertigkeitswert aus Abschnitt 11.6. Der Spieler greift nur bei Prioritätenwechsel ein. Das verhindert, dass die Versorgungspflicht zur Klick-Buchhaltung wird, und verlagert die Spielerrolle konsequent auf Management/Priorisierung statt Ausführung — siehe auch Abschnitt 10 für dasselbe Prinzip bei Haushaltsgütern.

**Darstellungsempfehlung:** Fokus auf die wirtschaftliche/rechtliche Ebene (Arbeitskraft, Versorgungspflicht, Rechtsstatus, Revolte-Risiko, Freilassung) statt auf explizite Gewalt- oder Ausbeutungsdarstellung im Detail — das bildet die historische Realität wirtschaftlich korrekt ab, ohne dass das Spiel den Fokus auf grafische Gewalt verlagert, der ohnehin nicht zu deinem Prinzip "Kampf sekundär" passt.

**Konkrete Balancing-Werte (⚠️ Spieldesign-Konstruktion, nicht historisch belegt):**

*Versorgungsbedarf pro Þræll (analog zu freiem Haushaltsmitglied):*

| Bedarf | Basiswert | Modifikator |
|---|---|---|
| Nahrung | 1 Versorgungseinheit (VE)/Tag | Winter (Þorri/Góa): +50 %; schwere Arbeit (Waldrodung, Torfstechen): +20 % |
| Kleidung | 1 Satz/Jahr | Eskalationsstufen bei Mangel siehe Abschnitt 10 (Vitalitätskopplung) |
| Unterkunft | Anteiliger Platz im Langhaus oder eigene einfache Unterkunft | Kein laufender Verbrauch, bindet aber Baukapazität |

*Unmuts-/Revolte-Risiko-Kurve (0–100-Skala, gespeist aus Unterversorgung, Überarbeitung, harter Behandlung):*

| Unmutsniveau | Konsequenz pro Saison |
|---|---|
| 0–30 | Kein Risiko |
| 30–60 | 2–5 % Fluchtversuch-Wahrscheinlichkeit |
| 60–90 | 10–20 % Fluchtversuch-Wahrscheinlichkeit |
| 90–100 | Ereignis garantiert nächste Saison (Flucht oder Aufstand — Hjörleifr-Fall oben als Extremvorlage) |

Zusätzlicher Multiplikator: Mehr als drei Þrælar am selben Hof erhöhen das Risiko zusätzlich (leichtere Koordination untereinander), analog zum historischen Fall (zehn Þrælar gemeinsam gegen Hjörleifr).

**UI-Prinzip — Wert bleibt verdeckt:** Der exakte Unmutswert ist reine Backend-Größe für die Revolte-Logik und wird dem Spieler **nie** als Zahl oder Balken angezeigt — sonst ließe sich knapp unter der Schwelle "optimieren". Sichtbar sind nur drei bis vier grobe Verhaltensstufen ohne Zahlenbezug (z. B. "wirkt ruhig" → "wirkt angespannt, murrt bei Zusatzaufgaben" → "wirkt zunehmend aufgebracht, Arbeitstempo sinkt sichtbar" → "unberechenbar"), transportiert über NPC-Verhalten, Körperhaltung und Bemerkungen anderer Gesindemitglieder statt HUD-Element. Die Übergänge zwischen den Stufen sollten zusätzlich leicht zufällig streuen (z. B. ±5–10 Punkte um die eigentliche Schwelle), damit auch aufmerksame Spieler die exakten Grenzen nicht durch Beobachtung zurückrechnen können. Das ist zugleich historisch stimmig — auch ein realer Hofbesitzer hätte nie eine exakte Prozentzahl gekannt, nur beobachtbares Verhalten.

---

## 5. Bauwesen: Holz, Stein und Torf

| Material | Verfügbarkeit | Isolation | Arbeitsaufwand | Bemerkung |
|---|---|---|---|---|
| Holz | Import/Treibholz, knapp | Sehr gut | Mittel | Statusgut, primär für Dachstuhl/Rahmen |
| Stein | Lokal (Basalt), reichlich | Mittel | Hoch | Aufgeschichtet, nicht gemörtelt |
| Torf | Reichlich, kostenlos | Gut | Niedrig | Begrenzte Haltbarkeit, regelmäßige Instandhaltung nötig |

**Historischer Standard:** Hybridbauweise — Steinsockel, Torfwände, importierter Holzrahmen (Skáli/Langhaus). **Reine Steinbauweise** als teure, arbeitsintensive Sonderoption.

**Trockenmauerwerk / Broch-Technik (aufgetürmt statt gemörtelt):** Flache Bruchsteine ohne Mörtel schichten, durch Kragsteinbau (Corbelling — jede Lage kragt leicht nach innen, wie bei den eisenzeitlichen schottischen Brochs) tragfähig aufeinandertürmen. Keine Präzisionswerkzeuge nötig, nur sorgfältige Steinauswahl und Geduld.

⚠️ Historische Einordnung: Brochs selbst sind eisenzeitlich (ca. 300 v. Chr. – 100 n. Chr.) und zur Landnahmezeit bereits Ruinen, aber die Trockenmauer-/Kragstein-**Technik** war in Schottland/auf den Hebriden/Orkney weiterhin bekannt. Da ein relevanter Teil der isländischen Siedler nachweislich über die Hebriden/Orkney/Irland kam, ist dies plausibel als **Herkunfts-Vorteil** modellierbar (siehe Abschnitt 11.1). Ergebnis: freistehende, mörtelfreie Steinstrukturen — geeignet für Vorratstürme (feuer-/schädlingssicher, spart Holz) oder Ausguckposten zur Küstenüberwachung.

---

## 6. Energiequellen: Geothermie und das Prinzip "plausible Technologie"

**Designprinzip:** Jede Technologie ist im Spiel verfügbar, wenn sie ausschließlich era-typische Materialien (Holz, Stein, Ton, Eisen/Bronze, Wolle/Leder) und einfache mechanische Grundprinzipien (Hebel, Rolle, Rinne, Gefälle) nutzt — unabhängig davon, ob konkret in Island belegt. Ausgeschlossen: Elektrizität und alles, was Präzisionsmechanik oder Druckgefäße voraussetzt (z. B. Dampfkraft).

### 6.1 Zwei geologisch unterschiedliche Quellentypen

<cite index="67-1">Island unterscheidet geologisch zwischen rund 250 Niedertemperatur-Gebieten mit insgesamt etwa 800 heißen Quellen, verteilt über das ganze Land, und nur rund 30 Hochtemperatur-Gebieten, die ausschließlich innerhalb der aktiven Vulkanzone liegen.</cite>

| Typ | Wasserchemie | Vorkommen | Nutzung im Spiel |
|---|---|---|---|
| Niedertemperatur | <cite index="66-1">Alkalisch, geringe Konzentration gelöster Mineralstoffe</cite> | Häufig, über ganz Island verteilt | Baden, Kochen, Raumheizung, **und** direkter Handwerkskontakt (Färben, Gerben, Brauen) |
| Hochtemperatur | <cite index="67-1">Schlammtöpfe, Schwefeltöpfe, Fumarolen; deutlich höhere Konzentration gelöster Chemikalien</cite> | Selten, nur in der aktiven Vulkanzone | Schwefelabbau (Exportgut, siehe 14.3); für direkten Wasserkontakt bei Handwerk ungeeignet, allenfalls indirekte Erhitzung per Dampf-/Wasserbad-Prinzip |

**Designkonsequenz:** Die Standortwahl bei der Landnahme wird dadurch strategisch differenziert — ein Hof auf einem Niedertemperatur-Feld erhält die volle Bandbreite an Nutzungsmöglichkeiten, ein Hof nahe einem Hochtemperatur-Feld stattdessen Zugang zum wertvollen Schwefel-Exportgut. Zwei unterschiedliche, beide plausible wirtschaftliche Strategien statt einer einheitlichen "Geothermie ist gut"-Mechanik.

**Historisch belegt (bzw. als langlebige Praxis plausibel bis in die Landnahmezeit zurückreichend):**
- Warmbaden in heißen Quellen (laugar)
- Kochen/Backen in Erdgruben nahe Quellen (Teig/Fleisch/Fisch im geothermisch erwärmten Boden gegart, Prinzip des bis heute praktizierten hverabrauð) sowie direktes Garen in kochend heißen Quellen

**Plausibel, aber unbelegt (im Spiel nutzbar, jeweils an Niedertemperatur-Quellen):**
- Rinnen-/Kanalsystem zur Fußboden-/Raumheizung mit Quellwasser (einfacher als ein römischer Hypocaustum, da keine Feuerung nötig ist) — spart Feuerholz/Torf
- Beheizte Anzuchtflächen/Torfgewächshäuser — verlängert die Vegetationsperiode
- Vorgewärmtes Wasser für Handwerk (Wollwäsche, Färben, Gerben, Brauen) — siehe 6.1, nur an Niedertemperatur-Standorten sinnvoll

**Bewusst ausgeschlossen:** Schmelzen/Schmieden (Temperaturlimit der meisten Quellen), jede Form von Dampf-/Druckkraft, direkter Handwerkskontakt an Hochtemperatur-/Schwefelquellen (siehe 6.1).

**Designkonsequenz Kochen:** Erdgruben-/Quellkochen ist feuerholzfrei, aber langsam (Stunden statt Minuten) und ortsgebunden — echter Standortvorteil für geothermisch aktive Parzellen bei der Landnahme-Wahl.

---

## 7. Kalender- und Wettersystem

**Empfehlung:** Authentischer altisländischer Kalender — zwei Hauptjahreszeiten (sumar/vetur), unterteilt in benannte Monate (u. a. Einmánuður, Harpa, Heyannir, Gormánuður, Þorri, Góa).

**Kopplung an Kernmechaniken:**
- **Segelfenster:** Mai–September
- **Heyannir (Heuernte):** Winterheu entscheidet über Viehbestand; verregnete Ernte = reales Verlustrisiko
- **Vogelfang (inkl. Puffins):** Nur zur Brutsaison (ca. Mai–August) und bei stabilem Wetter sicher durchführbar
- **Geothermie-Nutzung:** Wirtschaftlich am relevantesten im Winter (Þorri/Góa), wenn Feuerholz knapp und Kälte am größten

⚠️ Unsicherheit: Detailtiefe des Wettersystems (Ereignis-Wahrscheinlichkeit vs. simulierte Fronten) ist noch offen, siehe Abschnitt 12.

---

## 8. Zentrales Designrisiko: Freiheit vs. simulierbare Balance

Waldschwund und Bodenerosion sind kein abstraktes Balancing-Tool, sondern das reale historische Kernthema der isländischen Landnahmezeit — Realismus und Spielbalance fallen zusammen statt zu konkurrieren.

---

## 9. Kernschleifen (Gameplay Loops)

| Loop | Beschreibung | Wirtschaftsanbindung |
|---|---|---|
| Sammeln | Treibholz, Beeren, Kräuter, Algen, Vogeleier | Rohstoffbasis, saisonal/riskant begrenzt |
| Fischerei | Küsten- und Bootsfischerei (Kabeljau) | Nahrung + Exportgut (Stockfisch) |
| Vogelfang/Robbenjagd | Klippen (inkl. Puffins), Küste | Nahrung, Federn/Daunen, Fett/Öl |
| Weidewirtschaft | Schafe, Rinder, Ziegen, Pferde | Wolle (Vaðmál), Milch/Skyr, Fleisch, Häute |
| Steinbruch | Basalt-Gewinnung, Trockenmauerwerk | Baumaterial, arbeitsintensiv |
| Geothermie-Nutzung | Rinnenbau, Bäder, Kochgruben, Gewächshäuser | Feuerholz-Ersparnis, verlängerte Anbausaison |
| Handwerk | Wollverarbeitung, Schmiedearbeit, Holzverarbeitung, Töpferei, Böttcherei | Wertschöpfung, Fertigkeitswachstum |
| Hausbau | Torf/Stein/Holz-Hybridsystem | Bindet Ressourcen, schafft Kapazität |
| Landnahme/Landwirtschaft | Landbeanspruchung, Gerstenanbau, Heuwirtschaft | Grundversorgung, stark saisonal |
| Handel/Transport | Küstenboote, saisonale Überfahrt | Kernmechanik für Import/Export |
| Kampf (sekundär) | Hofverteidigung, Landgrenzkonflikte, Thing-Rechtsstreit als Alternative | Risikofaktor, kein Progressionsziel |

---

## 10. Alltagsausrüstung und Haushaltsgegenstände

Ein vollständiges Siedlerleben braucht neben Nahrung/Unterkunft auch die alltäglichen Gebrauchsgegenstände — diese sollten eigene, kleinteilige Produktionsketten haben, nicht nur "im Hintergrund vorhanden sein":

| Kategorie | Beispiele | Herstellung/Handwerk |
|---|---|---|
| Behälter | Eimer, Fässer, Kübel, Schöpfkellen | Böttcherei (Daubenbau aus Holzstreifen + Reifen), alternativ Leder-/Birkenrindenbehälter (holzsparend!) |
| Essgeschirr/Besteck | Löffel (Holz/Horn), Messer, Schüsseln, Trinkhörner | Schnitzen, Hornverarbeitung, Schmiedearbeit (Messerklingen) |
| Kleidung | Wollkleidung, Umhänge, Schuhe (Leder), Kopfbedeckung | Weberei, Näherei, Gerberei/Lederverarbeitung |
| Textilzubehör | Spinnrocken, Webgewicht, Nadeln (Knochen/Bronze) | Eigene kleine Produktionskette, oft Frauendomäne historisch (Rollenzuweisung optional spielbar) |
| Werkzeug | Äxte, Sicheln, Grabstöcke, Angelhaken, Netze | Schmiedearbeit (Importeisen), Holzbearbeitung |
| Lagerung/Konservierung | Salzfässer, Trockengestelle für Fisch/Fleisch, Räucherkammern | Nutzt vorhandene Rohstoffe, eigener Konservierungs-Loop |
| Beleuchtung | Talglampen (Tran/Fett-basiert) | Nebenprodukt der Fisch-/Robbenverarbeitung |

**Designkonsequenz:** Diese Gegenstände nutzen sich ab (Verschleiß-/Reparaturmechanik) und schaffen dadurch einen kontinuierlichen Grundbedarf an Handwerksarbeit — unabhängig von größeren Bauprojekten. Gleichzeitig entsteht ein Holzspar-Anreiz: Eimer/Behälter aus Leder oder Birkenrinde statt Dauben-Holzfässern sind eine bewusste, spielmechanisch sinnvolle Alternative bei Holzknappheit.

**Steuerungsprinzip — Aggregatbestand statt Einzelobjekt-Verwaltung:** Jede Kategorie wird als Soll-/Ist-Mengenbestand pro Haushalt geführt (z. B. "6 Löffel benötigt, 5 vorhanden"), nicht als einzeln verwaltetes Objekt. Ein Fehlbestand ist **kein Hard-Fail** — Nahrungsaufnahme, Werkzeugnutzung etc. funktionieren weiter, notfalls improvisiert —, sondern senkt graduell einen Moral-/Komfort-Wert des Haushalts. Das folgt demselben Frostpunk-Prinzip wie die Aufgabenzuweisung in Abschnitt 4.5 (aggregierter Zustand statt Einzelaktion) und lässt sich direkt mit der dortigen Versorgungspflicht verknüpfen: niedrige Moral senkt plausibel Arbeitsleistung oder erhöht Krankheitsrisiko, statt sofort tödlich zu sein. Nachschub läuft über dieselbe Aufgabenzuweisung — der Spieler priorisiert "mehr Löffel", die zugewiesene Arbeitskraft erledigt es eigenständig.

**UI-Prinzip — auch hier bleibt der exakte Wert verdeckt:** Wie beim Unmutswert der Þrælar (Abschnitt 4.5) wird der Moral-/Komfort-Wert dem Spieler nicht als Zahl oder Balken angezeigt, sondern nur über drei bis vier grobe, verhaltensbasierte Stufen wahrnehmbar (z. B. "Haushalt wirkt zufrieden" → "man behilft sich, kleine Klagen" → "spürbarer Unmut im Haushalt" → "offene Beschwerden"), mit leicht zufällig streuenden Übergängen zwischen den Stufen. Konsequente Anwendung desselben Verschleierungsprinzips auf beide Zufriedenheits-Ressourcen im Spiel, statt Sonderregel nur für Þrælar.

**Ausnahme — Kleidung koppelt direkt an Vitalität, nicht an Moral:** Anders als Behälter oder Essgeschirr ist Kleidungsmangel im subarktischen Klima kein Komfortproblem, sondern ein Überlebensrisiko, und wird entsprechend nicht über den Moral-Malus, sondern direkt über die Vitalitäts-/Gesundheitsressource aus Abschnitt 11.5 abgebildet — dieselbe Ressource, die schon durch Unterversorgung und Kälte sinkt:

1. **Leichter Kleidungs-Fehlbestand:** erhöhter Vitalitätsverlust bei Kälteexposition, Erkältungsrisiko — reversibel
2. **Deutlicher Fehlbestand, insbesondere im Winter (Þorri/Góa, siehe Abschnitt 7):** Erfrierungsrisiko — kann zu **dauerhaften** Attributsschäden führen (z. B. erfrorene Finger → permanenter Geschick-Malus)
3. **Schwerer/andauernder Fehlbestand:** Tod durch Unterkühlung, konsistent mit der emergenten Mortalitätslogik aus 11.5 — kein separater Zufallswürfel, sondern direkte Konsequenz der Vitalitätsressource

Historisch plausibel: Erfrierung und Unterkühlung waren reale Todesursachen der Zeit, und Wollkleidung/Vaðmál war ohnehin bereits die zentrale Wirtschaftsware des Spiels (Abschnitt 4.2, 14.1) — die Kopplung verstärkt die Bedeutung dieser Ressource zusätzlich, statt eine neue Mechanik einzuführen.

---

## 11. Charaktersystem (eigenständig, kein DSA-Bezug)

### 11.1 Herkunft und Startbedingungen

Der Charakter startet mit unterschiedlichen Fertigkeits- und Ausrüstungsprofilen je nach Herkunft — historisch plausibel, da Landnahme-Siedler aus verschiedenen Regionen mit unterschiedlichem Vorwissen kamen:

| Herkunft | Startvorteile | Startnachteile |
|---|---|---|
| **Norwegischer Bauer (Norðmaðr)** | Starke Vieh-/Ackerbaukenntnis, gute Handelskontakte nach Norwegen (bessere Importpreise/-verfügbarkeit), Küstenschifffahrt | Keine Trockenmauer-/Kragsteintechnik, geringere Starterfahrung mit Klippen-Vogelfang |
| **Hebridisch-gälischer Siedler (Vestmaðr)** | Trockenmauer-/Broch-Technik von Anfang an, Klippenklettern/Vogelfang, andere Textilmustertechniken | Schwächere direkte Handelskontakte nach Norwegen, ggf. geringeres Startkapital |
| **Färöisch/Shetland-Herkunft** | Ausgeprägte Klippen-/Seevogel-Fangkenntnis, robuste Seemannschaft | Kleineres Startvieh, wenig Ackerbauerfahrung |
| **Freigelassener/einfacher Siedler** | Breite Grundfertigkeiten (Generalist), keine Spezialisierungsnachteile | Deutlich geringeres Startkapital/-vieh/-werkzeug |

⚠️ Hinweis zur historischen Einordnung: Ein Teil der Landnahme-Bevölkerung bestand aus unfreien Personen (Sklaverei/Thralldom war historische Realität der Wikingerzeit). Für die "Freigelassener"-Option empfiehlt sich eine sachlich-neutrale Darstellung als sozioökonomischer Startpunkt (weniger Ressourcen), ohne die Herkunft als Charaktereigenschaft zu werten — reines Startkapital-Balancing, keine erzählerische Abwertung.

### 11.2 Attribute

Sechs Grundattribute, eigenständig benannt (keine DSA-Übernahme):

- **Kraft** — körperliche Stärke, Tragkapazität, Kampfschaden
- **Geschick** — Feinmotorik, Handwerkspräzision, Klettern
- **Zähigkeit** — Ausdauer, Kälte-/Krankheitsresistenz
- **Wahrnehmung** — Fischschwärme/Wetterzeichen erkennen, Spurenlesen, Navigation
- **Verstand** — Lerngeschwindigkeit, Verhandlungsgeschick, Technikverständnis
- **Wille** — Führungsstärke, Thing-Rede, Standfestigkeit

### 11.3 Fertigkeiten und Wachstum

Fertigkeiten sind an Attribute gekoppelt und wachsen **durch Anwendung** (learning-by-doing, keine Punktekauf-Levelung), mit abnehmendem Grenzertrag bei hohem Fertigkeitsniveau. Kategorien orientieren sich an den Kernschleifen aus Abschnitt 9: Landwirtschaft, Handwerk (je Gewerk einzeln: Schmieden, Weberei, Böttcherei, Steinbau, Holzbau, Lederverarbeitung), Nahrungsbeschaffung (Fischfang, Vogelfang, Jagd), Seefahrt (Küstenschifffahrt, Steuermannskunst), Handel/Verhandeln, Kampf (sekundär), Soziales/Recht (Thing-Rede, Führung).

### 11.4 Probenmechanik (eigenständig)

Vorschlag: **2W10-Bell-Curve** (Summe zweier zehnseitiger Würfel, glockenförmige Verteilung um 11) gegen Zielwert (Attribut + Fertigkeitsstufe) — erzeugt verlässlichere, weniger extreme Ergebnisse als ein einzelner W20 und unterscheidet sich strukturell klar vom DSA-3W20-System.

---

### 11.5 Demografie und Mehrgenerationen-System

**Historische Datenlage:** Lebenserwartung "bei Geburt" ist irreführend, weil sie durch extreme Säuglings-/Kindersterblichkeit stark nach unten verzerrt wird. Aussagekräftiger für Spieldesign ist die Sterblichkeitskurve nach Alter:

<cite index="58-1">An einem Gräberfeld im isländischen Keldudalur (frühes 11. Jh., vermutlich drei bis fünf Generationen einer Bauernfamilie) waren 16 von 26 Kinderskeletten unter einem Jahr alt — hohe Säuglingssterblichkeit war die Norm.</cite> <cite index="58-1">Wer die Kindheit überlebte, dessen Sterberisiko stieg jedoch typischerweise erst wieder nach dem 30. Lebensjahr deutlich an.</cite> <cite index="56-1">Eine Auswertung von 943 erwachsenen Skeletten aus drei Gräberfeldern in Ribe (Dänemark, Wikinger- bis nachmittelalterliche Zeit) ergab ein mittleres Sterbealter von 38,5 Jahren bei Männern und 38,6 Jahren bei Frauen, wobei die meisten Todesfälle zwischen 25 und 55 Jahren auftraten.</cite>

⚠️ Wichtige Einordnung: <cite index="56-1">Diese ~38-Jahre-Zahl beschreibt das Sterbealter bereits Erwachsener, nicht die Lebenserwartung bei Geburt — vormoderne Gesellschaften hatten hohe Säuglings-/Kindersterblichkeit, die den Durchschnitt stark drückt, auch wenn viele Erwachsene bis in ihre 40er, 50er und darüber hinaus lebten.</cite> Für die spezifische Landnahme-Frühphase (874–930) liegen keine eigenen Skelettdaten vor (die zitierten Studien stammen aus der Zeit kurz danach) — ⚠️ ein plausibler, aber unbelegter Zusatzfaktor ist ein erhöhtes Sterberisiko in den ersten ein bis zwei Siedlungsjahren durch Nahrungsunsicherheit in unerprobtem Terrain, analog zu bekannten Mustern bei anderen Erstsiedler-Kolonien (z. B. Jamestown, Plymouth).

**Designkonsequenz — Mehrgenerationen-System nach Die-Gilde-Prinzip:**

- Der Spieler führt eine **Hofdynastie**, nicht eine einzelne unsterbliche Figur. Tod eines Charakters (Krankheit, Unfall, Kindbett-Risiko bei Frauen, Winterhärte, selten Gewalt) ist normaler Spielverlauf, kein Game Over.
- **Erbfolge:** Beim Tod übernimmt ein Erbe (Kind, Ehepartner, ggf. adoptierter/freigelassener Þræll) den Hof — mit teilweise vererbten Fertigkeiten (z. B. durch Miterleben/Anlernen im Haushalt) und vollständig vererbtem Besitz/Ausrüstung.
- **Heirat als Mechanik:** Verbindet zwei Herkunfts-/Fertigkeitsprofile (Abschnitt 11.1) — eine Ehe zwischen norwegischer und hebridisch-gälischer Linie vermischt z. B. Vieh-/Ackerbauwissen mit Trockenmauer-Technik in der nächsten Generation. Das macht Heiratsentscheidungen wirtschaftlich relevant, nicht nur narrativ.
- **Altersabhängige Leistungskurve:** Kraft/Ausdauer erreichen Höchstwerte im jungen Erwachsenenalter, Verstand/Wille können mit Erfahrung weiter steigen — jüngere Erben sind körperlich leistungsfähiger, ältere Charaktere wertvoller für Handel/Führung/Thing-Rede.
- **Saisonale Mortalitätsspitzen:** Verknüpft direkt mit dem Kalendersystem aus Abschnitt 7 — Wintermonate (Þorri/Góa) mit erhöhtem Krankheits-/Unterernährungsrisiko bei knappen Vorräten, analog zur realen Bedeutung einer gelungenen Heyannir-Ernte fürs Überleben.

**Mechanik-Empfehlung — emergent statt gewürfelt:** Kein separater, isolierter Alters-Todeswürfel. Stattdessen Gesundheit/Vitalität als Ressource, die durch Unterversorgung, Kälte und Krankheit sinkt; Alter moduliert nur die Resilienz gegen diese Faktoren, tötet nicht direkt. Die historisch hohe Sterblichkeit der Gründungsjahre (siehe ⚠️-Hinweis oben zu Erstsiedler-Kolonien) ergibt sich dadurch automatisch aus dem Zusammenspiel bereits vorhandener Systeme: fehlende eingespielte Vorratslage im ersten Winter (Abschnitt 7), Versorgungspflicht bei Þrælar (Abschnitt 4.5), und Geothermie-Zugang als echter Überlebensvorteil bei der Standortwahl (Abschnitt 6) — Realismus und Balance fallen damit auch bei der Mortalität zusammen statt zu konkurrieren (vgl. Prinzip aus Abschnitt 8).

---

### 11.6 Konkrete Fertigkeits-Startwerte je Herkunft

Skala 0–10 (0 = keine Erfahrung, 10 = Expertenniveau — realistische Startwerte liegen zwischen 0 und 4, Attribut-Baseline 5/10 für einen durchschnittlichen Erwachsenen).

**Attribute (Deltas ggü. Baseline 5):**

| Attribut | Norwegischer Bauer | Hebridisch-gälisch | Färöisch/Shetland | Freigelassener |
|---|---|---|---|---|
| Kraft | 6 | 5 | 5 | 5 |
| Geschick | 5 | 6 | 5 | 5 |
| Zähigkeit | 6 | 5 | 6 | 5 |
| Wahrnehmung | 5 | 6 | 6 | 5 |
| Verstand | 5 | 5 | 5 | 5 |
| Wille | 5 | 5 | 5 | 6 |

Freigelassener bekommt bewusst keinen Attribut-Malus — nur den Wille-Bonus als Resilienzfaktor, konsistent mit Abschnitt 11.1: Herkunft als Startkapital-Faktor, nicht als Charakterschwäche.

**Fertigkeiten (Startwerte 0–4):**

| Fertigkeit | Norwegischer Bauer | Hebridisch-gälisch | Färöisch/Shetland | Freigelassener |
|---|---|---|---|---|
| Ackerbau | 4 | 2 | 1 | 2 |
| Viehzucht | 4 | 2 | 2 | 2 |
| Schmieden | 1 | 1 | 0 | 1 |
| Weberei/Textil | 2 | 3 | 1 | 2 |
| Holzbau | 3 | 1 | 1 | 2 |
| Steinbau (inkl. Trockenmauerwerk) | 1 | 4 | 2 | 2 |
| Lederverarbeitung | 2 | 1 | 2 | 2 |
| Böttcherei | 2 | 1 | 1 | 2 |
| Fischfang | 2 | 3 | 4 | 2 |
| Vogelfang/Klettern | 1 | 3 | 4 | 1 |
| Robbenjagd | 1 | 2 | 3 | 1 |
| Küstenschifffahrt | 3 | 2 | 4 | 1 |
| Steuermannskunst | 1 | 1 | 2 | 0 |
| Handel/Verhandeln | 3 | 1 | 1 | 1 |
| Kampf (Nahkampf) | 1 | 2 | 1 | 1 |
| Thing-Rede/Führung | 2 | 1 | 1 | 0 |

**Designnotiz zu Thing-Rede/Führung beim Freigelassenen:** Der Wert 0 ist kein Balancing-Nachteil, sondern rechtlich begründet — Unfreie hatten historisch kein Rederecht am Thing. Der Wert steigt logisch erst nach der (im Spiel erreichbaren) Freilassung von 0 auf einen normalen Lernwert, was dem Freilassungs-Ausbaupfad aus Abschnitt 4.5 eine spürbare mechanische statt nur narrative Konsequenz gibt.

---

## 12. Technische Architektur

**Engine:** Unity (C#) — ausgereiftes Tilemap-/Isometrie-Tooling, gute Skalierbarkeit für viele NPC-Wirtschaftsagenten.

**Darstellung:** Echtes 3D-Terrain-Mesh mit orthografischer Kamera im Isometrie-Winkel (klassisch 30°/45°) statt klassischer 2D-Diamant-Tile-Isometrie — vermeidet sichtbare Nahtstellen an unregelmäßigen realen Küstenlinien (Fjorde), da Gelände über Splat-Map-Materialien (Gras/Lava/Fels/Torf) statt diskreter Tile-Sprites nahtlos ineinander blendet. Erfüllt den Anspruch an echte Geländehöhen (Lavafelder, Fjorde, Hänge) strukturell statt kaschiert.

### 12.1 Kartengrundlage: reales Terrain statt fiktiver Karte

**Datenquelle:** <cite index="81-1">ÍslandsDEM v1.0 — ein Höhenmodell in 10 m Auflösung (an einzelnen Stellen auch 2 m), produziert vom isländischen Vermessungsamt Landmælingar Íslands zusammen mit PGC, unter CC-BY-4.0-Lizenz frei verfügbar</cite> — über dem.lmi.is/mapview zugänglich, deutlich präziser als generische globale Datensätze (SRTM/Copernicus, ~30 m).

**Regionswahl:** Nicht ganz Island modellieren, sondern eine Region mit dichter Landnahme-Saga-Geografie eingrenzen — z. B. Südwesten (Faxaflói-Bucht/Reykjavík, Þingvellir, optional bis Hjörleifshöfði/Vestmannaeyjar).

**Historisch bedeutsame Orte:** Landnámabók-Ortsindex als Primärquelle, ergänzt durch Örnefnasjá (isländisches Flurnamen-Register mit Koordinaten, ebenfalls über Landmælingar Íslands).

**Pipeline DEM → Unity:**
1. QGIS (kostenlos, Open Source): Zielregion aus ÍslandsDEM zuschneiden, auf gewünschte Auflösung reprojizieren/downsamplen
2. Als 16-Bit-Graustufen-Heightmap (PNG/RAW) exportieren
3. In Unity: Terrain-Komponente → "Import Raw" direkt mit der Heightmap füttern

**Offene technische Fragen:** Umfang der Wettersimulation, Detailtiefe der Zwei-Ebenen-Wirtschaftssimulation (Tick-Frequenz für entfernte Höfe), Speicherformat für Weltzustand bei prozeduraler Geländegenerierung.

### 12.2 Sounddesign: Musik ohne Copyright-Risiko

**Faktenlage:** Von der Landnahmezeit selbst (874–930) ist keine Musik überliefert — Notenschrift existierte im Norden noch nicht. <cite index="82-1">Die älteste bekannte nordische Handschrift mit moderner Notenschrift für Mehrstimmigkeit ist das Credo im Munkþverárbók von 1473</cite> — rund 550 Jahre nach dem Spiel-Setting. Kommerzielle "Wikingermusik" (Wardruna, Danheim, Heilung u. Ä.) ist moderne Komposition mit historisierendem Anstrich und urheberrechtlich geschützt — nicht nutzbar.

**Nächstbeste gemeinfreie Quelle:** <cite index="87-1">Der Volkskundler Bjarni Þorsteinsson dokumentierte zwischen 1906 und 1909 in "Íslenzk þjóðlög" hunderte traditionelle isländische Melodien</cite> (Rímur, Tvísöngur, Kvæði) — über 115 Jahre alt und damit gemeinfrei. Das ist die historisch am weitesten zurückreichende, tatsächlich noch greifbare isländische Musiktradition, wenn auch erst im 20. Jahrhundert notiert, nicht Landnahme-zeitgenössisch.

**Designempfehlung:** Kommunikation als "inspiriert von überlieferter isländischer Musiktradition", nicht als "historisch korrekte Landnahme-Musik" — ehrlicher und ohne überzogenen Authentizitätsanspruch.

**Technische Umsetzung:** Melodien aus Þorsteinssons Sammlung digitalisieren (Notenbild → MusicXML/MIDI), mit period-passenden Instrumentensamples (Lyra, Knochenflöte, Rahmentrommel) neu einspielen, und über eine Audio-Middleware wie **FMOD** oder **Wwise** adaptiv in Unity einbinden — MIDI-Rohwiedergabe ist für interaktive/zustandsabhängige Spielmusik heute nicht mehr Stand der Technik.

---

## 13. Rechtliche Einordnung

- Setting, Geographie, Geschichte Islands: vollständig gemeinfrei
- Kein DSA-Bezug mehr im Charaktersystem — eigenständige Attribute, eigenständige Probenmechanik
- Sagas (Landnámabók, Íslendingasögur) sind gemeinfreie historische Quellen; bei Nutzung moderner Übersetzungen auf Gemeinfreiheit/Alter der Übersetzung achten

---

## 14. Historische Referenzwerte für Warenwerte

Quellenlage: Grágás (isländisches Gesetzeswerk, älteste Teile 1117 verschriftlicht, Regelungen z. T. bis in die 920er zurückreichend), Sagas, sowie moderne Auswertungen (u. a. Hurstwic, Jesse Byock "Viking Age Iceland"). Werte gelten für **frühes 11. Jahrhundert Island** — für die Landnahmezeit selbst (874–930) ⚠️ nur als Näherung zu verwenden, da die erste schriftliche Fixierung erst ca. 200 Jahre später erfolgte und sich Kurse nachweislich über Zeit verschoben.

### 14.1 Grundeinheiten

| Einheit | Definition | Bemerkung |
|---|---|---|
| Eyrir (Unze, Pl. aurar) | ≈ 27 g Silber | Basis-Recheneinheit |
| Mark | 8 aurar | Größere Silbereinheit |
| Lögeyrir (Rechtsunze) | 6 Ellen Wollstoff (vaðmál), 2 Ellen breit | Kurs zur Silberunze schwankte: 8:1 (11. Jh.) → 7,5:1 (12. Jh.) → 6:1 (13. Jh.) |
| Kúgildi (Kuhwert) | = 1 Kuh = 6 trächtige Schafe | Der zentrale Wertmaßstab der isländischen Wirtschaft |
| Hundrað | 120 Ellen Vaðmál | = Wert einer Kuh oder sechs Schafe |

### 14.2 Austauschkurse (Island, frühes 11. Jh., nach Grágás/Hurstwic)

| Gut | Wert in Silberunzen | Quelle |
|---|---|---|
| 1 Unze Gold | 8 Unzen Silber | Grágás-Kurse, Hurstwic |
| 1 Milchkuh | 2 Unzen Silber | Grágás-Kurse, Hurstwic |
| 1 Schaf | ⅓ Unze Silber (24 Schafe = 8 Unzen) | Grágás-Kurse, Hurstwic |
| 1 Elle Wollstoff (vaðmál, 2 Ellen breit) | ≈ 1/18 Unze Silber (144 Ellen = 8 Unzen) | Grágás-Kurse, Hurstwic |
| Prunkschwert (Königsgeschenk-Qualität) | ≈ 32 Unzen Silber (= 16 Milchkühe) | Laxdæla saga, Kap. 13 |

⚠️ Zur Einordnung: Der Schwertwert stammt aus einem einzelnen, expliziten Saga-Beleg (königliches Geschenk) und repräsentiert die Oberklasse — ein gewöhnliches Arbeitsschwert lag vermutlich deutlich darunter, dazu liegt aber kein belastbarer Einzelwert vor. Eisen war generell teuer (aufwendige Verhüttung), daher waren auch einfache Werkzeuge/Waffen relativ hochpreisig gegenüber landwirtschaftlichen Gütern.

⚠️ Ein historischer Datenpunkt betrifft den Wert eines erwachsenen männlichen Sklaven (12 Unzen Silber, Hurstwic/Grágás) — Sklaverei war reale Institution der Wikingerzeit. Ich würde empfehlen, diesen Wert **nicht** als spielbare Handelsware zu implementieren (kein Kauf/Verkauf von Personen als Gameplay-Mechanik), auch wenn er als historischer Fakt dokumentiert ist — vergleichbar mit der Praxis vieler historischer Strategiespiele, das Thema atmosphärisch/narrativ zu adressieren statt als Wirtschaftsgut zu simulieren.

### 14.3 Regionale Exportgüter (nach Hurstwic, Wikingerzeit)

| Region | Exportgüter |
|---|---|
| Island | Fisch, Tran/Tierfett, Wollstoff & Kleidung, **Schwefel**, Falken |
| Norwegen | Bauholz, Eisen, **Speckstein**, Wetzsteine, Gerste, Teer |
| Grönland | Walross-Elfenbein, Pelze, Felle, Wolle |
| Shetland | Speckstein |

Zwei nützliche Ergänzungen ggü. dem bisherigen Konzept:
- **Schwefel** als Island-Exportgut schließt direkt an das Geothermie-Thema an (Abschnitt 6) — Schwefelgewinnung an vulkanisch aktiven Standorten wird ein plausibler, authentischer Nebenerwerb für Höfe mit entsprechender Lage
- **Speckstein** (Norwegen/Shetland-Import) ist relevant für Kochgeschirr (specksteinerne Kochtöpfe waren im Norden verbreitet, hitzebeständig, langlebig) — passt gut in Abschnitt 10 (Alltagsausrüstung) als Alternative/Ergänzung zu Ton- oder Holzgeschirr

### 14.4 Designkonsequenz für die Wirtschaftssimulation

Empfehlung: **Kúgildi (Kuhwert) als interne Baseline-Recheneinheit** für die Preis-Engine verwenden (nicht direkt Silber, da Silber in Island tatsächlich knapp war — siehe Abschnitt 4). Alle anderen Güter lassen sich über die obigen Kursrelationen konsistent daraus ableiten, und das Spiel bekommt dadurch eine authentische, nicht-monetäre Wirtschaftslogik (Naturaltausch/Warengeld statt Münzwirtschaft) als Kernmechanik statt als Flavor.

### 14.6 Preistabelle für Güter aus Abschnitt 9/10

Die verankerten Werte oben (14.1/14.2) sind historisch belegt. Für die übrigen Güter aus Abschnitt 9/10 gibt es keine Primärquellen — hier folgen Schätzwerte nach Material-/Arbeitsaufwand-Analogie, ⚠️ klar als Spielbalance-Konstruktion gekennzeichnet, nicht als historischer Fakt.

**Geschätzte Analogiewerte, nach Material-/Arbeitsintensität:**

| Gut | Kúgildi (geschätzt) | Begründung |
|---|---|---|
| 1 Ballen Vaðmál (6 Ellen = 1 Lögeyrir) | 0,17 | Direkt aus verankerter Basis (14.2) |
| 1 Löffel (Holz/Horn) | 0,01 | Gering Material, gering Arbeit |
| 1 Eimer (Dauben) | 0,05 | Böttcherei-Arbeit, moderat Holz |
| 1 Axt/Werkzeug (Eisen) | 0,3–0,5 | Eisen war im Norden aufwendig herzustellen und daher teuer — alles mit hohem Eisenanteil entsprechend hochpreisig |
| 1 Fuhre Import-Bauholz (kleine Menge) | 0,5–1,0 | Statusgut, siehe Abschnitt 4.2/5 |
| Hof-Grundausstattung (komplettes Starterset Haushaltsgüter) | 2–3 | Aggregiert aus Einzelposten |

**Methodik-Empfehlung für die Umsetzung:** Statt jedes Item einzeln von Hand zu bepreisen, in der Spieldaten-Datenbank eine Formel je Kategorie hinterlegen: *Wert = Materialkosten (in Kúgildi-Äquivalent der Rohstoffe) + Arbeitszeit × Lohnsatz-Multiplikator*, mit Eisen- und Holz-Multiplikator bewusst hoch angesetzt (Knappheit, siehe Abschnitt 4.2/5). Das hält die Preislogik intern konsistent, auch wenn später neue Items ergänzt werden.

---

## 15. Offene Punkte für nächste Vertiefung

- Detailausarbeitung Bausystem (Statik-Simulation ja/nein?)
- Wettersystem-Architektur (siehe Abschnitt 12)
- NPC-Anstellung/Gesinde (Húskarlar, Vinnufólk) — jetzt auch im Zusammenhang mit Þræll-System aus Abschnitt 4.5 zu betrachten
- Transportmitteldetails (Küstenboot vs. Knorr — Kapazität/Risiko/Kosten)
- ~~Konkrete Fertigkeitsliste mit Startwerten je Herkunft (Tabelle)~~ — erledigt, siehe Abschnitt 11.6
- Umfang des Geothermie-Systems: nur Gebäudeheizung/Kochen, oder auch handwerkliche Prozesse?
- ~~Feinjustierung der Kúgildi-basierten Preistabelle für alle Güter aus Abschnitt 9/10~~ — erledigt, siehe Abschnitt 14.6
- ~~Konkrete Balancing-Werte für Þræll-Versorgungsbedarf und Revolte-Risiko-Kurve~~ — erledigt, siehe Abschnitt 4.5

---

## 16. Marktumfeld: Vergleichbare Spiele

Kurze Bestandsaufnahme, was bereits existiert, um Überschneidungen und Alleinstellungsmerkmale einzuordnen.

| Spiel | Studio | Setting/Fokus | Abgleich mit diesem Konzept |
|---|---|---|---|
| **Landnama** | Sonderland Games | Wikingerzeit-Island, rundenbasiertes Hex-Crawl-Survival, Single-Resource-Ökonomie ("Heart"), bewusst gewaltfrei | Thematisch am nächsten, aber deutlich abstrakter/einfacher — kein Multi-Güter-System, keine Norwegen-Handelsroute, keine Fertigkeitstiefe |
| **Land of the Vikings** | Laps Games / Iceberg Interactive | Colony-Sim, generisches Wikinger-Setting, Produktionsketten, Generationenmechanik | Mechanisch näher an der gewünschten Wirtschaftstiefe, aber mit Fantasy-Elementen (Odin-/Freya-Monumente, "Blitze aus Asgard") und Raubzug-/Kampf-Zentrierung — widerspricht beiden Kernprinzipien "keine Fantasy" und "Kampf sekundär" |
| **Viking City Builder** | PlayWay S.A. | Noch unveröffentlicht, Aufbau/Handel mit Raubzug-Elementen | Zu früh in der Entwicklung für detaillierten Vergleich, aber angekündigter Kampf-/Raubzug-Fokus deutet in dieselbe Richtung wie Land of the Vikings |

**Einordnung:** Es gibt eine reale, aber kleine Nische für Wikingerzeit-Wirtschaftssimulationen. Keines der bestehenden Spiele verbindet historisch-spezifisches Island-Landnahme-Setting, echte Multi-Güter-NPC-Wirtschaftssimulation und konsequent sekundären (statt zentralen) Kampf — die Kombination bleibt eine Lücke.

**Namenskollision:** Der bisherige Arbeitstitel "Landnám" kollidiert mit dem existierenden Sonderland-Spiel "Landnama" (isländische Schreibweise ohne Akzent, aber phonetisch identisch). Da beide im selben Genre/Setting spielen, empfehle ich einen eigenständigen Titel, um Verwechslung in Suche/Marketing zu vermeiden — "Öræfi" (isländisch für "Wildnis/unbewohntes Land") ist ein Vorschlag, kein endgültiger Titel.
