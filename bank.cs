using System;
using System.Threading;

class Account //класс для банка
{
    private decimal balance; //переменная для хранения счета
    private readonly object sinh = new object(); //объект для синхронизации потоков

    public decimal Balance
    {
        get
        {
            lock (sinh)//блокировка баланса для того, чтобы потоки не делали гонку, пока выполняется блок кода
            {
                return balance;
            }
        }
    }

    public void popolnenie(decimal sum) //метод для поплнения счета
    {
        if (sum <= 0) //проверка чтобы сумма была положиетльной
            throw new ArgumentException("сумма должна быть положительной");

        lock (sinh)//блокирует доступ для безопасного изменения
        {
            balance += sum; //увеличивает баланс на сумму пополнения
            Console.WriteLine($"пополнение: {sum}. баланс: {balance}");
            Monitor.Pulse(sinh); // сигнал, что баланс пополнен
        }
    }

    public void snyatie(decimal suum)//метод для снятия с счета
    {
        if (suum <= 0)//проверка положиетлльной суммы
            throw new ArgumentException("сумма снятия должна быть положительной");

        lock (sinh)
        {
            while (balance < suum)//проверка достаточно ли средств
            {
                Console.WriteLine($"пополнение для снятия {suum} баланс: {balance}");
                Monitor.Wait(sinh); //ожидание пополнения
            }

            balance -= suum;//уменьшает баланс на сумму снятия
            Console.WriteLine($"снятие: {suum} остаток: {balance}");
        }
    }
}

class Program
{
    private static Random random = new Random(); //генерация случайнвх чисел

    static void Main(string[] args)
    {
        Account account = new Account();

        // поток для пополнения счета
        Thread potokpopolneniya = new Thread(() =>// определение нового потока
        {
            for (int i = 0; i < 20; i++)
            {
                decimal a = (decimal)(random.Next(1, 1000));//генерация случайной суммы пополнения
                account.popolnenie(a);//вызывает метод для пополнения
            }
        });

        potokpopolneniya.Start();//запуск потока снятия

        for (int i = 0; i < 5; i++)
        {
            decimal b = (decimal)(random.Next(1, 1000));//генерация суммы снятия
            account.snyatie(b);//вызов метода для уменьшения баланса
            Console.WriteLine($"Остаток на счете: {account.Balance}");
        }

        potokpopolneniya.Join(); //ожидание завершения потока пополнения
    }
}
