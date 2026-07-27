Repository<Book> bookRepository = new();
Repository<Library> libraryRepository = new();

bookRepository.Add(new Book("Clean Code"));
bookRepository.Add(new Book("C# programming"));

libraryRepository.Add(new Library("Central Library"));
libraryRepository.Add(new Library("AAUP Library"));


IReadOnlyList<Book> books = bookRepository.GetAll();

Console.WriteLine(books.Count);
Console.WriteLine(books[0].Title);
//error 

// Restricts T to reference types because repositories are intended to store objects, not value types.
class Repository<T> where T : class
{
    private List<T> items = new();

    public void Add(T item)
    {
        items.Add(item);
    }

    public IReadOnlyList<T> GetAll()
{
    return items.AsReadOnly();
}

    public T? Find(Predicate<T> predicate)
    {
        return items.Find(predicate);
    }
}

class Book
{
    public string Title { get; set; }

    public Book(string title)
    {
        Title = title;
    }
}

class Library
{
    public string Name { get; set; }

    public Library(string name)
    {
        Name = name;
    }
}