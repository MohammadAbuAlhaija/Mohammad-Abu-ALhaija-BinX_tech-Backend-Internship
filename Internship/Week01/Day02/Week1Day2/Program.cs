using System;

int age = 22;
double height = 1.76;
bool isStudent = true;


string MyName = "Mohammad";
int []numbers = { 1, 2, 3, 4, 5 };
List<string> fruits = new List<string> { "Apple", "Banana", "Cherry" };

Console.WriteLine("Value Types:");
Console.WriteLine($"age type: {age.GetType()}");
Console.WriteLine($"height type: {height.GetType()}");
Console.WriteLine($"isStudent type: {isStudent.GetType()}");

Console.WriteLine();

Console.WriteLine("Reference Types:");
Console.WriteLine($"MyName type: {MyName.GetType()}");
Console.WriteLine($"numbers type: {numbers.GetType()}");
Console.WriteLine($"fruits type: {fruits.GetType()}");

Console.WriteLine();

void CopyBehavior()
{
    int x = 10;
    int y = x;
    Console.WriteLine($"Before Change: x = {x}, y = {y}");
    y = 20;
    Console.WriteLine($"After Change:  x = {x}, y = {y}");
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------");

    List<string> list1 = new List<string> { "Apple", "Banana" };
    List<string> list2 = list1;
    Console.WriteLine($"Before Change: list1[0] = {list1[0]}, list2[0] = {list2[0]}");
    list2[0] = "Orange";
    Console.WriteLine($"After Change:  list1[0] = {list1[0]}, list2[0] = {list2[0]}");
}
Console.WriteLine();

CopyBehavior();

string GradeClassifiecation(int score)
{
    string grade = score switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F"
    };
    return grade;
}

Console.WriteLine();

int score = 85;
Console.WriteLine($"Score: {score} -> {GradeClassifiecation(score)}");

/************************************************
 * 
 * Nullable Reference Types
 * 
 ************************************************/
Console.WriteLine();

Console.Write("Enter your name: ");
string? name = Console.ReadLine();

if (name is not null)
{
    Console.WriteLine($"Hello, {name}!");
}
else
{
    Console.WriteLine("No name was entered.");
}