// See https://aka.ms/new-console-template for more information
using System.Text;

Console.WriteLine("Розыгрыш от дружок кружок!");

var path = "E:\\publish\\bob217downloads\\production\\priz";
var file =  "users.txt";// "debug.txt";
var fullPath = Path.Combine(path, file);

var users = File.ReadAllLines(fullPath);
for (int userIndex = 0; userIndex < users.Length; userIndex++)
{
    string? user = users[userIndex];
    var maskedUser = new StringBuilder();
    maskedUser.Append((userIndex + 1) + "  ");
    for (int i = 0; i < user.Length; i++)
    {
        if (i % 2 == 0)
        {
            maskedUser.Append("*");
        }
        else
        {
            maskedUser.Append(user[i]);
        }
    }
    Console.WriteLine(maskedUser.ToString());
}

var random = new Random();
//int[] usersWin = new int[users.Length];
//for(var i = 0; i < 1000000; i++)
//{
//   var userIndex = random.Next(0, users.Length);
//    usersWin[userIndex]++;
//}

//for (int userIndex = 0; userIndex < users.Length; userIndex++)
//{
//    Console.WriteLine((userIndex+1) + " "+ usersWin[userIndex]);
//}

var winIndex = random.Next(0, users.Length);
var delay = 30;
for(var i = 0; i < delay; i++)
{
    Thread.Sleep(1000);
    Console.WriteLine((delay-i) +"...♥");
}
Thread.Sleep(1000);
Console.WriteLine("♥♥♥♥♥!!!ура победитель!!!♥♥♥♥");
Console.WriteLine((winIndex+1)+ " "+ users[winIndex]);
Console.WriteLine("♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");