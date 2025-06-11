using AssessmentInnocel.Models;
using AssessmentInnocel.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentInnocel.Controllers;

[ApiController]
[Route("[controller]")]
public class SubmitTrxMessageController : ControllerBase
{
    public SubmitTrxMessageController()
    {

    }

    [HttpGet]
    public ActionResult<List<Transaction>> GetAll() => TransactionService.GetAll();

    [HttpGet("{partnerRefNo}")]
    public ActionResult<Transaction> Get(string partnerRefNo)
    {
        var transaction = TransactionService.Get(partnerRefNo);

        if (transaction == null)
            return NotFound();

        return transaction;
    }


    [HttpPost]
    public IActionResult Update(Transaction transaction)
    {
        Console.WriteLine("\n\n\n===> INSIDE the HttpPost Update method of SubmitTrxMessageController\n");
        var transactionInMemory = TransactionService.GetAll();
        var query = transactionInMemory.Select(t => new { t.PartnerRefNo, t.PartnerKey, t.Sig }).ToList();

        // Console.WriteLine("List the available transaction obj's related properties.");
        // foreach (var i in query)
        // {
        //     Console.WriteLine($"{i.PartnerKey} {i.PartnerRefNo} {i.Sig}");
        // }

        /*
            Question 2 - 1 Validate the partner or signature mismatch 
        */
        var matchSigOrPartnerKey = query.Where(i => i.Sig == transaction.Sig)
                    .Where(j => j.PartnerKey == transaction.PartnerKey)
                    .FirstOrDefault();

        // Console.WriteLine("MATCHED the sig: ");
        // Console.WriteLine(matchSigOrPartnerKey);

        if (matchSigOrPartnerKey is null)
        {
            var msg = "Access Denied!";
            return StatusCode(400, new { result = 0, resultmessage = msg });
        }

        /*
            Question 2 - 2 Validate totalamount field with the sum of each item price
        */
        var sum = 0L;
        if (transaction.Items is not null)
        {
            var queryItems = transaction.Items.ToList();
            foreach (var i in queryItems)
            {
                //  Console.WriteLine($"qty: {i.Qty} unitprice: {i.UnitPrice} " +
                //     $"the totalAmount for this item is MYR {i.Qty * i.UnitPrice / 1000.0 }");
                sum += i.Qty * i.UnitPrice;
            }
            if (sum != transaction.TotalAmount)
            {
                var msg = "Invalid Total Amount!";
                return StatusCode(400, new { result = 0, resultmessage = msg });
            }
        }

        /*
            Question 2 - 3 Validate the timestamp field whether it is exceed the server time +-5min
        */
        string isoTimestamp = transaction.Timestamp;
        DateTime parsedTimeUtc = DateTime
                    .Parse(isoTimestamp, null,
                        System.Globalization.DateTimeStyles.AdjustToUniversal);
        DateTime currentUtcTime = DateTime.UtcNow;
        TimeSpan timeDifference = (parsedTimeUtc - currentUtcTime).Duration();

        if (!(timeDifference <= TimeSpan.FromMinutes(5)))
        {
            var msg = "Expired.";
            return StatusCode(400, new { result = 0, resultmessage = msg });
        }


        var existingTransaction = TransactionService.Get(matchSigOrPartnerKey.PartnerRefNo);
        if (existingTransaction is null)
            return NotFound();

        /*
            Question 3: Discount Rules Implementation
        */
        // --- 1,000 cents = MYR 10.00, 10,000 cents = MYR 100.00
        var totalAmount = transaction.TotalAmount;

        Console.WriteLine($"\n\nWhen totalAmount is {totalAmount} cents in MYR");

        var noDiscountRule = totalAmount < 20000;
        var fivePercentDiscountRule = (totalAmount >= 20000) && (totalAmount <= 50000);
        var sevenPercentDiscountRule = (totalAmount >= 50100) && (totalAmount <= 80000);
        var tenPercentDiscountRule = (totalAmount >= 80100) && (totalAmount <= 120000);

        Console.WriteLine($"no discount: {noDiscountRule}");
        Console.WriteLine($"5% discount: {fivePercentDiscountRule}");
        Console.WriteLine($"7% discount: {sevenPercentDiscountRule}");
        Console.WriteLine($"10% discount: {tenPercentDiscountRule}");

        var percent = 0.0;
        if (fivePercentDiscountRule)
            percent = 0.05;
        else if (sevenPercentDiscountRule)
            percent = 0.07;
        else if (tenPercentDiscountRule)
            percent = 0.1;
        else if (noDiscountRule)
            percent = 0.0;

        // Conditional discounts
        Func<long, bool> isPrime = n =>
            n > 1 &&
            (n == 2 || n % 2 != 0) &&
            Enumerable.Range(3, (int)Math.Sqrt(n) - 2)
                      .Where(x => x % 2 != 0)
                      .All(x => n % x != 0);

        Console.WriteLine($"A) is totalAmount Prime Number: {isPrime(totalAmount)}");
        Console.WriteLine($"B) is totalAmount > 50000 cents in MYR: {totalAmount > 50000}");
        Console.WriteLine($"C) is totalAmount > 90000 cents in MYR: {totalAmount > 90000}");
        Console.WriteLine($"D) is totalAmount end with digit 5: {totalAmount % 10 == 5}");

        var moreThanMYR500AndPrimeNum = (totalAmount > 50000) && isPrime(totalAmount);
        var moreThanMYR900AndEndWithDigit5 = (totalAmount > 90000) && (totalAmount % 10 == 5);

        Console.WriteLine($"\nA) and B) - plus 8% discount: {moreThanMYR500AndPrimeNum}");
        Console.WriteLine($"C) and D) - plus 10% discount: {moreThanMYR900AndEndWithDigit5}");

        if (moreThanMYR500AndPrimeNum)
            percent += 0.08;
        else if (moreThanMYR900AndEndWithDigit5)
            percent += 0.10;

        var totalDiscount = totalAmount * percent;
        var finalAmount = totalAmount * (1 - percent);

        // Cap on Maximum Discount
        var twentyPercentOfTotalAmount = totalAmount * (1 - 0.20);

        if (totalDiscount > twentyPercentOfTotalAmount)
        {
            totalDiscount = totalAmount * (1 - 0.20);
        }

        TransactionService.Update(transaction);

        return StatusCode(200,
            new
            {
                result = 1,
                totalamount = transaction.TotalAmount,
                totaldiscount = totalDiscount,
                finalamount = finalAmount
       });
    }
};