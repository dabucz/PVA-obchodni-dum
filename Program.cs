using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("===== Store Checkout Queue Simulation =====\n");

        int numberOfCheckouts = 4;
        int simulationDurationMinutes = 60;
        double averageArrivalIntervalSeconds = 30.0;
        double averageItemsInCart = 15.0;

        if (args.Length > 0)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Error: Invalid arguments. Expected 4 parameters.");
                Console.WriteLine("Usage: dotnet run <number_of_checkouts> <simulation_duration_minutes> <average_arrival_interval_seconds> <average_items_in_cart>");
                Console.WriteLine($"\nUsing default values: {numberOfCheckouts} checkouts, {simulationDurationMinutes} minutes, {averageArrivalIntervalSeconds}s arrival interval, {averageItemsInCart} items");
            }
            else
            {
                try
                {
                    numberOfCheckouts = int.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture);
                    simulationDurationMinutes = int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture);
                    averageArrivalIntervalSeconds = double.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
                    averageItemsInCart = double.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture);

                    if (numberOfCheckouts < 1 || simulationDurationMinutes < 1 || averageArrivalIntervalSeconds <= 0 || averageItemsInCart <= 0)
                    {
                        Console.WriteLine("Error: All parameters must be positive values (checkouts and duration >= 1).");
                        return;
                    }
                }
                catch
                {
                    Console.WriteLine("Error: Invalid argument format. Please provide valid numbers.");
                    return;
                }
            }
        }
        else
        {
            Console.WriteLine($"Using default parameters:");
        }

        Console.WriteLine($"Checkouts: {numberOfCheckouts}");
        Console.WriteLine($"Simulation duration: {simulationDurationMinutes} minutes");
        Console.WriteLine($"Average customer arrival interval: {averageArrivalIntervalSeconds} seconds");
        Console.WriteLine($"Average items in cart: {averageItemsInCart}");
        Console.WriteLine("\nStarting simulation...\n");

        SimulationManager simulation = new SimulationManager(
            numberOfCheckouts,
            simulationDurationMinutes,
            averageArrivalIntervalSeconds,
            averageItemsInCart
        );

        Stopwatch stopwatch = Stopwatch.StartNew();
        simulation.RunSimulation();
        stopwatch.Stop();

        Console.WriteLine("\n========================================");
        Console.WriteLine("Simulation completed.");
        Console.WriteLine("========================================");
        simulation.DisplayStatistics();
        
        if (stopwatch.Elapsed.TotalSeconds < 1.0)
        {
            Console.WriteLine($"Execution time: {stopwatch.Elapsed.TotalMilliseconds:F2} milliseconds");
        }
        else
        {
            Console.WriteLine($"Execution time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        if (stopwatch.Elapsed.TotalSeconds > 5.0)
        {
            Console.WriteLine($"\nWarning: Simulation took {stopwatch.Elapsed.TotalSeconds:F2} seconds (exceeded 5s threshold).");
            Console.WriteLine("Consider reducing simulation duration.");
        }

        Console.WriteLine("\n========================================");
    }
}

class Customer
{
    public int Id { get; set; }
    public int Items { get; set; }
    public double ArrivalTime { get; set; }
    public double QueueEntryTime { get; set; }
    public double ServiceStartTime { get; set; }
    public double ServiceEndTime { get; set; }

    public double WaitingTime => ServiceStartTime - QueueEntryTime;
    public double ServiceTime => ServiceEndTime - ServiceStartTime;
}

class Checkout
{
    public int Id { get; set; }
    public bool IsBusy { get; set; }
    public Queue<Customer> Queue { get; set; }
    public Customer? CurrentCustomer { get; set; }
    public double TotalBusyTime { get; set; }
    public double LastServiceEndTime { get; set; }

    public Checkout(int id)
    {
        Id = id;
        IsBusy = false;
        Queue = new Queue<Customer>();
        CurrentCustomer = null;
        TotalBusyTime = 0.0;
        LastServiceEndTime = 0.0;
    }

    public int QueueLength => Queue.Count + (IsBusy ? 1 : 0);
}

class SimulationEvent : IComparable<SimulationEvent>
{
    public double Time { get; set; }
    public EventType Type { get; set; }
    public Customer? Customer { get; set; }
    public Checkout? Checkout { get; set; }

    public int CompareTo(SimulationEvent? other)
    {
        if (other == null) return 1;
        return Time.CompareTo(other.Time);
    }
}

enum EventType
{
    CustomerArrival,
    ServiceComplete
}

class SimulationManager
{
    private Random randomGenerator;
    private double currentTime;
    private List<Checkout> checkouts;
    private List<Customer> allCustomers;
    private PriorityQueue<SimulationEvent> eventQueue;
    private int nextCustomerId;

    private int numberOfCheckouts;
    private double simulationDuration;
    private double averageArrivalInterval;
    private double averageItems;

    private const double FixedServiceTime = 20.0;
    private const double TimePerItem = 3.0;
    private const double ShoppingTimePerItem = 2.0;

    private List<double> queueLengthSamples;
    private int maxQueueLength;

    public SimulationManager(int checkouts, int durationMinutes, double avgArrivalInterval, double avgItems)
    {
        randomGenerator = new Random();
        currentTime = 0.0;
        this.checkouts = new List<Checkout>();
        allCustomers = new List<Customer>();
        eventQueue = new PriorityQueue<SimulationEvent>();
        nextCustomerId = 1;
        queueLengthSamples = new List<double>();
        maxQueueLength = 0;

        numberOfCheckouts = checkouts;
        simulationDuration = durationMinutes * 60.0;
        averageArrivalInterval = avgArrivalInterval;
        averageItems = avgItems;

        for (int i = 0; i < numberOfCheckouts; i++)
        {
            this.checkouts.Add(new Checkout(i + 1));
        }
    }

    public void RunSimulation()
    {
        ScheduleCustomerArrival(0.0);

        while (eventQueue.Count > 0)
        {
            SimulationEvent nextEvent = eventQueue.Dequeue();
            currentTime = nextEvent.Time;

            if (currentTime > simulationDuration)
            {
                break;
            }

            SampleQueueLengths();

            if (nextEvent.Type == EventType.CustomerArrival)
            {
                HandleCustomerArrival(nextEvent.Customer);
            }
            else if (nextEvent.Type == EventType.ServiceComplete)
            {
                HandleServiceComplete(nextEvent.Checkout);
            }
        }
    }

    private void ScheduleCustomerArrival(double time)
    {
        double interArrivalTime = GenerateExponential(averageArrivalInterval);
        double arrivalTime = time + interArrivalTime;

        if (arrivalTime > simulationDuration)
        {
            return;
        }

        int items = GeneratePoissonItems(averageItems);
        double shoppingTime = items * ShoppingTimePerItem;

        Customer customer = new Customer
        {
            Id = nextCustomerId++,
            Items = items,
            ArrivalTime = arrivalTime,
            QueueEntryTime = arrivalTime + shoppingTime
        };

        allCustomers.Add(customer);

        SimulationEvent arrivalEvent = new SimulationEvent
        {
            Time = customer.QueueEntryTime,
            Type = EventType.CustomerArrival,
            Customer = customer
        };

        eventQueue.Enqueue(arrivalEvent);

        ScheduleCustomerArrival(arrivalTime);
    }

    private void HandleCustomerArrival(Customer? customer)
    {
        if (customer == null) return;
        
        Checkout selectedCheckout = SelectCheckoutWithShortestQueue();
        selectedCheckout.Queue.Enqueue(customer);

        if (!selectedCheckout.IsBusy)
        {
            StartService(selectedCheckout);
        }
    }

    private void StartService(Checkout checkout)
    {
        if (checkout.Queue.Count == 0)
        {
            return;
        }

        Customer customer = checkout.Queue.Dequeue();
        checkout.IsBusy = true;
        checkout.CurrentCustomer = customer;
        customer.ServiceStartTime = currentTime;

        double serviceTime = FixedServiceTime + customer.Items * TimePerItem;
        double serviceEndTime = currentTime + serviceTime;
        customer.ServiceEndTime = serviceEndTime;

        checkout.TotalBusyTime += serviceTime;
        checkout.LastServiceEndTime = serviceEndTime;

        SimulationEvent serviceCompleteEvent = new SimulationEvent
        {
            Time = serviceEndTime,
            Type = EventType.ServiceComplete,
            Checkout = checkout
        };

        eventQueue.Enqueue(serviceCompleteEvent);
    }

    private void HandleServiceComplete(Checkout? checkout)
    {
        if (checkout == null) return;
        
        checkout.IsBusy = false;
        checkout.CurrentCustomer = null;

        if (checkout.Queue.Count > 0)
        {
            StartService(checkout);
        }
    }

    private Checkout SelectCheckoutWithShortestQueue()
    {
        return checkouts.OrderBy(c => c.QueueLength).First();
    }

    private void SampleQueueLengths()
    {
        int totalInQueue = checkouts.Sum(c => c.Queue.Count);
        queueLengthSamples.Add(totalInQueue);

        if (totalInQueue > maxQueueLength)
        {
            maxQueueLength = totalInQueue;
        }
    }

    private double GenerateExponential(double mean)
    {
        double u = randomGenerator.NextDouble();
        return -mean * Math.Log(1 - u);
    }

    private int GeneratePoissonItems(double lambda)
    {
        double L = Math.Exp(-lambda);
        double p = 1.0;
        int k = 0;

        do
        {
            k++;
            p *= randomGenerator.NextDouble();
        } while (p > L);

        return Math.Max(1, k - 1);
    }

    public void DisplayStatistics()
    {
        List<Customer> servedCustomers = allCustomers.Where(c => c.ServiceEndTime > 0).ToList();

        if (servedCustomers.Count == 0)
        {
            Console.WriteLine("\nNo customers were served during the simulation.");
            return;
        }

        double avgWaitTime = servedCustomers.Average(c => c.WaitingTime);
        double avgQueueLength = queueLengthSamples.Count > 0 ? queueLengthSamples.Average() : 0;
        double totalUtilization = checkouts.Sum(c => c.TotalBusyTime) / (numberOfCheckouts * simulationDuration) * 100.0;

        Console.WriteLine($"Total customers arrived: {allCustomers.Count}");
        Console.WriteLine($"Customers served: {servedCustomers.Count}");
        Console.WriteLine($"Number of checkouts: {numberOfCheckouts}");
        Console.WriteLine($"Average waiting time: {(avgWaitTime / 60.0):F2} minutes");
        Console.WriteLine($"Average queue length: {avgQueueLength:F2}");
        Console.WriteLine($"Maximum queue length: {maxQueueLength}");
        Console.WriteLine($"Checkout utilization: {totalUtilization:F1}%");

        Console.WriteLine("\nPer-checkout statistics:");
        foreach (var checkout in checkouts)
        {
            double utilization = (checkout.TotalBusyTime / simulationDuration) * 100.0;
            Console.WriteLine($"  Checkout {checkout.Id}: {utilization:F1}% utilization");
        }
    }
}

class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> data;

    public PriorityQueue()
    {
        data = new List<T>();
    }

    public void Enqueue(T item)
    {
        data.Add(item);
        int childIndex = data.Count - 1;

        while (childIndex > 0)
        {
            int parentIndex = (childIndex - 1) / 2;

            if (data[childIndex].CompareTo(data[parentIndex]) >= 0)
            {
                break;
            }

            T tmp = data[childIndex];
            data[childIndex] = data[parentIndex];
            data[parentIndex] = tmp;

            childIndex = parentIndex;
        }
    }

    public T Dequeue()
    {
        int lastIndex = data.Count - 1;
        T frontItem = data[0];
        data[0] = data[lastIndex];
        data.RemoveAt(lastIndex);

        lastIndex--;
        int parentIndex = 0;

        while (true)
        {
            int leftChildIndex = parentIndex * 2 + 1;
            if (leftChildIndex > lastIndex)
            {
                break;
            }

            int rightChildIndex = leftChildIndex + 1;
            int minIndex = leftChildIndex;

            if (rightChildIndex <= lastIndex && data[rightChildIndex].CompareTo(data[leftChildIndex]) < 0)
            {
                minIndex = rightChildIndex;
            }

            if (data[parentIndex].CompareTo(data[minIndex]) <= 0)
            {
                break;
            }

            T tmp = data[parentIndex];
            data[parentIndex] = data[minIndex];
            data[minIndex] = tmp;

            parentIndex = minIndex;
        }

        return frontItem;
    }

    public int Count => data.Count;
}
