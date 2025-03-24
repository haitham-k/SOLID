using Coffe;

Coffee coffee = new SimpleCoffee();
Console.WriteLine($"{coffee.GetDescription()} : ${coffee.GetCost()}");

coffee = new MilkDecorator(coffee);
Console.WriteLine($"{coffee.GetDescription()} : ${coffee.GetCost()}");

coffee = new SugarDecorator(coffee);
Console.WriteLine($"{coffee.GetDescription()} : ${coffee.GetCost()}");
Console.ReadLine();