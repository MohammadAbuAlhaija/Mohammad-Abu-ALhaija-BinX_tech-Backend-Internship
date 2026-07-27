using System;
using System.Collections.Generic;

BookDto book = new BookDto("imposible", "mohammad", 2008);

Console.WriteLine(book.Title);


Book book1 = new Book("imposible", "mohammad", 2008);

Console.WriteLine(book1.Title);
Console.WriteLine(book1.Author);
Console.WriteLine(book1.Year);

Book book2 = new Book("imposible", "mohammad", 2008);

book2.Title = "C# Programming";
book2.Year = 2025;

Console.WriteLine(book2.Title);
Console.WriteLine(book2.Author);
Console.WriteLine(book2.Year);

void PrintItem(IPrintable item)
{
    item.Print();
}

BookReport report = new BookReport();
LibraryCard card = new LibraryCard();

PrintItem(report);
PrintItem(card);

public interface IPrintable
{
    void Print();
}

public class BookReport : IPrintable
{
    public void Print()
    {
        Console.WriteLine("Printing Book Report...");
    }
}

public class LibraryCard : IPrintable
{
    public void Print()
    {
        Console.WriteLine("Printing Library Card...");
    }
}

public record BookDto(string Title, string Author, int Year);
public class Book
{
    private string? title;
    private string? author;
    private int year;

    public string? Title
    {
        get { return title; }
        set { title = value; }
    }

    public string? Author
    {
        get { return author; }
        set { author = value; }
    }

    public int Year
    {
        get { return year; }
        set { year = value; }
    }

    public Book(string title, string author, int year)
    {
        Title = title;
        Author = author;
        Year = year;
    }
}
public class Library
{
    public string? LibraryName;
    public List<Book> books = new List<Book>();

   public void AddBook(Book book)
    {
        books.Add(book);
    }

    public void removeBook(Book book)
    {
        books.Remove(book);
    }
}


