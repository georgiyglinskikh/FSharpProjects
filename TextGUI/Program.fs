open System
open System.Net
open System.Text.RegularExpressions
open System.Windows.Forms
open System.Drawing

let url = "http://www.textfiles.com/etext/AUTHORS/"

let http (fs: string) =
    let ws = new WebClient()
    ws.DownloadString fs

let RegEx reg str =
    let m = Regex.Matches(str, reg)
    [| for i in m -> i.Groups.[1].Value |]

let get path regex =
    path
    |> http
    |> RegEx regex

let getReferences path = get path @"<A HREF=""([^""]*)"">"
let getText path = get path @"(\w+)"

let getAuthors() = getReferences url
let getBooks author = getReferences (url + author + "/")
let getUrlBooksText author book = (url + author + "/" + book)



[<EntryPoint>]
let main argv =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault false

    let form =
        new Form(Size = Size(800, 600), Text = "Частота слов / Word frequency",
                 FormBorderStyle = FormBorderStyle.FixedDialog)

    let authorBox = new ComboBox(Location = Point(10, 10), Size = Size(100, 23))
    authorBox.Items.AddRange(Array.map (fun (a: string) -> box (string a.[0] + a.[1..].ToLower())) (getAuthors()))

    let bookBox = new ComboBox(Location = Point(120, 10), Size = Size(500, 23))
    bookBox.Enabled <- false

    let getTextButton = new Button(Location = Point(630, 10), Size = Size(140, 23), Text = "Обновить / Get data")
    getTextButton.Enabled <- false

    // Text box position = 0 + 10 + 23 + 10 = 43
    let textBox =
        new TextBox(Location = Point(10, 43), Size = Size(760, 500), ReadOnly = true, Multiline = true,
                    ScrollBars = ScrollBars.Vertical)
    textBox.Enabled <- false

    getTextButton.Click.Add(fun args ->
        if authorBox.Items.Contains(authorBox.Text) && bookBox.Items.Contains(bookBox.Text) then
            textBox.Enabled <- true
            let wordsByDecending =
                (getUrlBooksText (authorBox.Text.ToUpper()) bookBox.Text)
                |> getText
                |> Array.map (fun el -> el.ToLower())
                |> Array.groupBy id
                |> Array.sortByDescending (fun el -> (snd el).Length)
                |> Array.map (fun a -> ((fst a), (snd a).Length))
            textBox.Text <-
                Array.fold (fun a b ->
                    a + (b
                         |> fst
                         |> string) + " - " + (b
                                               |> snd
                                               |> string)
                    + "\r\n") "" wordsByDecending
        else
            textBox.Enabled <- false
            textBox.Text <- "")

    authorBox.SelectedValueChanged.Add(fun args ->
        if authorBox.Items.Contains authorBox.Text then
            bookBox.Enabled <- true
            bookBox.Items.AddRange(Array.map (fun (a: string) -> box (a)) (getBooks (authorBox.Text.ToUpper())))
        else
            bookBox.Enabled <- false
            getTextButton.Enabled <- false
            for i in 0 .. (bookBox.Items.Count - 1) do
                bookBox.Items.RemoveAt(i))

    bookBox.SelectedValueChanged.Add
        (fun args ->
            if bookBox.Items.Contains bookBox.Text then
                getTextButton.Enabled <- true
            else
                getTextButton.Enabled <- false)

    form.Controls.AddRange([| authorBox; bookBox; getTextButton; textBox |])

    Application.Run form
    0
