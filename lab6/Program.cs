using lab6.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace lab6
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new GymContext())
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine("=== Тестування міграції бази даних ===\n");

                try
                {
                    // Перевірка підключення
                    Console.WriteLine("Перевірка підключення до БД...");
                    bool canConnect = context.Database.CanConnect();
                    Console.WriteLine(canConnect ? "✓ Підключення успішне\n" : "✗ Помилка підключення\n");

                    if (!canConnect)
                    {
                        Console.WriteLine("Перевірте, чи запущений Docker контейнер з PostgreSQL");
                        return;
                    }

                    // --- ТЕСТ 1: Додавання зон ---
                    Console.WriteLine("--- ТЕСТ 1: Робота з FacilityZone ---");
                    
                    if (!context.Facilityzones.Any())
                    {
                        Console.WriteLine("Додавання нових зон...");
                        var zones = new[]
                        {
                            new Facilityzone { Name = "Pool Area" },
                            new Facilityzone { Name = "Cardio Zone" },
                            new Facilityzone { Name = "Strength Training Zone" },
                            new Facilityzone { Name = "Yoga Studio" }
                        };
                        
                        context.Facilityzones.AddRange(zones);
                        context.SaveChanges();
                        Console.WriteLine($"✓ Додано {zones.Length} зон\n");
                    }
                    else
                    {
                        Console.WriteLine($"✓ Зони вже існують: {context.Facilityzones.Count()} шт.\n");
                    }

                    // --- ТЕСТ 2: Додавання тарифних планів ---
                    Console.WriteLine("--- ТЕСТ 2: Робота з MembershipPlan ---");
                    
                    var basicPlan = context.Membershipplans.FirstOrDefault(p => p.Name == "Basic");
                    if (basicPlan == null)
                    {
                        Console.WriteLine("Створення тарифного плану 'Basic'...");
                        basicPlan = new Membershipplan
                        {
                            Name = "Basic",
                            DurationMonths = 1,
                            Price = 500
                        };
                        context.Membershipplans.Add(basicPlan);
                        context.SaveChanges();
                        Console.WriteLine("✓ План 'Basic' створено\n");
                    }
                    else
                    {
                        Console.WriteLine("✓ План 'Basic' вже існує\n");
                    }

                    var premiumPlan = context.Membershipplans.FirstOrDefault(p => p.Name == "Premium");
                    if (premiumPlan == null)
                    {
                        Console.WriteLine("Створення тарифного плану 'Premium'...");
                        premiumPlan = new Membershipplan
                        {
                            Name = "Premium",
                            DurationMonths = 6,
                            Price = 2500
                        };
                        context.Membershipplans.Add(premiumPlan);
                        context.SaveChanges();
                        Console.WriteLine("✓ План 'Premium' створено\n");
                    }
                    else
                    {
                        Console.WriteLine("✓ План 'Premium' вже існує\n");
                    }

                    // --- ТЕСТ 3: Зв'язок Many-to-Many (План ↔ Зони) ---
                    Console.WriteLine("--- ТЕСТ 3: Налаштування доступу планів до зон ---");
                    
                    // Перезавантажимо дані з навігаційними властивостями
                    basicPlan = context.Membershipplans
                        .Include(p => p.Zones)
                        .First(p => p.Name == "Basic");
                    
                    premiumPlan = context.Membershipplans
                        .Include(p => p.Zones)
                        .First(p => p.Name == "Premium");

                    var cardioZone = context.Facilityzones.First(z => z.Name == "Cardio Zone");
                    var yogaZone = context.Facilityzones.First(z => z.Name == "Yoga Studio");
                    var poolZone = context.Facilityzones.First(z => z.Name == "Pool Area");
                    var strengthZone = context.Facilityzones.First(z => z.Name == "Strength Training Zone");

                    // Basic має доступ тільки до Cardio та Yoga
                    if (basicPlan.Zones.Count == 0)
                    {
                        Console.WriteLine("Налаштування доступу для Basic плану...");
                        basicPlan.Zones.Add(cardioZone);
                        basicPlan.Zones.Add(yogaZone);
                        context.SaveChanges();
                        Console.WriteLine("✓ Basic: доступ до Cardio та Yoga\n");
                    }
                    else
                    {
                        Console.WriteLine($"✓ Basic вже має доступ до {basicPlan.Zones.Count} зон\n");
                    }

                    // Premium має доступ до всіх зон
                    if (premiumPlan.Zones.Count < 4)
                    {
                        Console.WriteLine("Налаштування доступу для Premium плану...");
                        premiumPlan.Zones.Clear();
                        premiumPlan.Zones.Add(cardioZone);
                        premiumPlan.Zones.Add(yogaZone);
                        premiumPlan.Zones.Add(poolZone);
                        premiumPlan.Zones.Add(strengthZone);
                        context.SaveChanges();
                        Console.WriteLine("✓ Premium: доступ до всіх 4 зон\n");
                    }
                    else
                    {
                        Console.WriteLine($"✓ Premium вже має доступ до {premiumPlan.Zones.Count} зон\n");
                    }

                    // --- ТЕСТ 4: Читання даних ---
                    Console.WriteLine("--- ТЕСТ 4: Звіт по тарифах та доступу ---");
                    
                    var allPlans = context.Membershipplans
                        .Include(p => p.Zones)
                        .ToList();

                    foreach (var plan in allPlans)
                    {
                        Console.WriteLine($"\n📋 Тариф: {plan.Name}");
                        Console.WriteLine($"   Ціна: {plan.Price} грн");
                        Console.WriteLine($"   Тривалість: {plan.DurationMonths} міс.");
                        Console.WriteLine($"   Доступні зони ({plan.Zones.Count}):");
                        
                        foreach (var zone in plan.Zones)
                        {
                            Console.WriteLine($"      • {zone.Name}");
                        }
                    }

                    // --- ДОДАТКОВИЙ ТЕСТ: Перевірка таблиці planaccess ---
                    Console.WriteLine("\n\n--- ДОДАТКОВИЙ ТЕСТ: Зміст таблиці planaccess ---");
                    
                    var planAccessQuery = context.Database
                        .SqlQuery<PlanAccessInfo>($"SELECT plan_id, zone_id FROM planaccess ORDER BY plan_id, zone_id")
                        .ToList();

                    Console.WriteLine($"Всього зв'язків у таблиці planaccess: {planAccessQuery.Count}");
                    foreach (var pa in planAccessQuery)
                    {
                        var planName = context.Membershipplans.Find(pa.plan_id)?.Name ?? "Unknown";
                        var zoneName = context.Facilityzones.Find(pa.zone_id)?.Name ?? "Unknown";
                        Console.WriteLine($"  Plan ID {pa.plan_id} ({planName}) -> Zone ID {pa.zone_id} ({zoneName})");
                    }

                    Console.WriteLine("\n\n=== Всі тести пройшли успішно! ===");
                    Console.WriteLine($"Загальна кількість зон: {context.Facilityzones.Count()}");
                    Console.WriteLine($"Загальна кількість планів: {context.Membershipplans.Count()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n✗ ПОМИЛКА: {ex.Message}");
                    Console.WriteLine($"Деталі: {ex.InnerException?.Message}");
                    Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
                }
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }

    // Допоміжний клас для читання з planaccess
    public class PlanAccessInfo
    {
        public int plan_id { get; set; }
        public int zone_id { get; set; }
    }
}
