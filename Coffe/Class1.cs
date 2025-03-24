using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffe
{
    using System;

    // Étape 1: Définir l'interface ou la classe abstraite (Composant)
    public abstract class Coffee
    {
        public abstract string GetDescription();
        public abstract double GetCost();
    }

    // Étape 2: Implémenter le composant concret
    public class SimpleCoffee : Coffee
    {
        public override string GetDescription()
        {
            return "Simple Coffee";
        }

        public override double GetCost()
        {
            return 1.0;
        }
    }

    // Étape 3: Créer la classe de base pour les décorateurs
    public abstract class CoffeeDecorator : Coffee
    {
        protected Coffee _coffee;

        public CoffeeDecorator(Coffee coffee)
        {
            _coffee = coffee;
        }

        public override string GetDescription()
        {
            return _coffee.GetDescription();
        }

        public override double GetCost()
        {
            return _coffee.GetCost();
        }
    }

    // Étape 4: Implémenter des décorateurs concrets
    public class MilkDecorator : CoffeeDecorator
    {
        public MilkDecorator(Coffee coffee) : base(coffee) { }

        public override string GetDescription()
        {
            return _coffee.GetDescription() + ", Milk";
        }

        public override double GetCost()
        {
            return _coffee.GetCost() + 0.5;
        }
    }

    public class SugarDecorator : CoffeeDecorator
    {
        public SugarDecorator(Coffee coffee) : base(coffee) { }

        public override string GetDescription()
        {
            return _coffee.GetDescription() + ", Sugar";
        }

        public override double GetCost()
        {
            return _coffee.GetCost() + 0.2;
        }
    }

}
