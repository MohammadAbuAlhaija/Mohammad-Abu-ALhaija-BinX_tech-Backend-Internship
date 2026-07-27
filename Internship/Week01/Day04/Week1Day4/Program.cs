using System;
using System.Collections.Generic;
using System.Linq;

List<Student> students = new()
{
    new Student("Mohammad", 95, "CSE"),
    new Student("Ahmad", 88, "CSE"),
    new Student("Omar", 72, "CS"),
    new Student("Yousef", 81, "IT"),
    new Student("Mahmoud", 67, "CSE"),
    new Student("Khaled", 90, "CSE"),
    new Student("Ali", 76, "IT"),
    new Student("Hamza", 84, "CS")
};

foreach (Student student in students)
{
    Console.WriteLine($"{student.Name} - {student.Grade} - {student.Major}");
}

//2.
var NumOfCseStudents = students
.Where(s => s.Major == "CSE")
.Count();

var ExcellentStudents = students
.Where(s => s.Grade >= 90)
.OrderByDescending(s => s.Grade)
.Select(s => s.Name)
.ToList();

Console.WriteLine($"Number of CSE Students: {NumOfCseStudents}");

Console.WriteLine("\nExcellent Students:");

foreach (var student in ExcellentStudents)
{
    Console.WriteLine(student);
}

//3.
string message = await LoadMessageAsync();

Console.WriteLine(message);

//4.
try
{
    Console.Write("Enter your age: ");
    int age = int.Parse(Console.ReadLine()!);

    Console.WriteLine($"Your age is {age}");
}
catch (FormatException)
{
    Console.WriteLine("Invalid input! Please enter a valid number.");
}

////////top level Statements///////

static async Task<string> LoadMessageAsync()
{
    await Task.Delay(3000);

    return "Students loaded successfully!";
}

public class Student
{
    public string Name{ get; set; }
    public double Grade{ get; set; }
    public string Major{ get; set; }

    public Student(string name, double grade, string major)
    {
        Name = name;
        Grade = grade;
        Major = major;
    }
}

