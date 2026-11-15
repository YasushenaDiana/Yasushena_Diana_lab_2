**YasushenaDiana_xml**

- Автор: Yasushena Diana
- група К-28
- Об'єктно-орієнтоване програмування
- Лабораторна робота № 2, варіант 16
- Тема: робота з файлами XML. "Кадри науковців (Звання)"

**Про програму**

- Відкриває XML-файл з даними про науковців: П.І.П., факультет, кафедра, науковий ступінь, вчене звання (із датами)
- Пошук за атрибутами трьома способами: SAX API (`XmlReader`), DOM API (`XmlDocument` + XPath), LINQ to XML (`XDocument`)
- Перетворення XML у HTML за XSL-документом (`XslCompiledTransform`)
- Інтерфейс україномовний, зроблений на .NET MAUI

**Шаблон проєктування "Стратегія"**

- `ISearchStrategy` - спільний інтерфейс пошуку
- `SaxSearchStrategy`, `DomSearchStrategy`, `LinqSearchStrategy` - три способи аналізу
- `XmlSearchContext` - контекст: `SetStrategy(...)`, `ExecuteSearch(...)`
- Обробники кнопок лише викликають методи `MainPage` (без антишаблону "магічна кнопка")

**Вимоги до XML (виконано у `TestFiles/scientists.xml`)**

- у вузлів `Scientist` різна кількість атрибутів (немає `Degree` чи `Department` у деяких)
- структура `Scientists / Scientist / Titles / Title` - більше ніж 2 рівні

**Вимоги до інтерфейсу**

- "підгрузка даних": значення для пошуку обираються лише з тих, що є у відкритому файлі
- підтвердження виходу: "Чи дійсно ви хочете завершити роботу з програмою?" - "Так" / "Ні"
  (кнопка "Вихід", меню, ⌘Q; на Windows також хрестик вікна; на macOS червона кнопка вимкнена)
- кнопка Clear очищує поле виводу і параметри пошуку
- повідомлення, якщо нічого не знайдено

**Структура**

- `Lab2.Core` - логіка: модель (`Scientist`, `AcademicTitle`, `SearchCriteria`), стратегії пошуку, `XmlAttributeReader`, `XslTransformer`
- `Lab2` - інтерфейс програми (`MainPage`)
- `Lab2.Tests` - модульні тести: усі три стратегії дають однаковий результат, у тому числі на некоректних файлах
- `TestFiles` - `scientists.xml` і `scientists.xsl`

**Вимоги**

- .NET 10 SDK
- компонент MAUI: `dotnet workload install maui`
- на macOS також Xcode

**Запуск**

- macOS: `dotnet build Lab2/Lab2.csproj -t:Run -f net10.0-maccatalyst`
- Windows: `dotnet build Lab2/Lab2.csproj -t:Run -f net10.0-windows10.0.19041.0`
- тести: `dotnet test Lab2.Tests`
- у програмі відкрити файл `TestFiles/scientists.xml`
