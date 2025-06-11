using System.Linq;
using AssessmentInnocel.CustomExtensions;
using AssessmentInnocel.Models;

namespace AssessmentInnocel.Services;

public static class TransactionService
{
    static List<Transaction> Transactions { get; }
    static List<ItemDetail> ItemDetails { get; }
    static List<ItemDetail> ItemDetails2 { get; }
    static TransactionService()
    {
        DateTime currentUtc = DateTime.UtcNow;
        DateTime fiveMinutesLater = currentUtc.AddMinutes(5);
        string timestampNow = currentUtc.ToString("o");
        string timestampPlus5 = fiveMinutesLater.ToString("o");

        ItemDetails = new List<ItemDetail>
        {
            new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen", Qty = 4, UnitPrice = 200 },
            new ItemDetail { PartnerItemRef = "i-00002", Name = "Ruler", Qty = 2, UnitPrice = 100 }
        };

        ItemDetails2 = new List<ItemDetail>
        {
            new ItemDetail { PartnerItemRef = "i-00003", Name = "Pencil", Qty = 3, UnitPrice = 150 },
            new ItemDetail { PartnerItemRef = "i-00004", Name = "Bag", Qty = 1, UnitPrice = 2800 }
        };

        Transactions = new List<Transaction>
        {
            new Transaction { PartnerKey = "FAKEGOOGLE", PartnerRefNo = "FG-00001",
                PartnerPassword = "FAKEPASSWORD1234", TotalAmount = 1000,
                Items = ItemDetails, Timestamp = timestampNow,
                Sig = "" },

            new Transaction { PartnerKey = "FAKEPEOPLE", PartnerRefNo = "FG-00002",
                PartnerPassword = "FAKEPASSWORD4578", TotalAmount = 0,
                Items = [], Timestamp = timestampPlus5,
                Sig = ""},

        };

        Transactions.Select(t =>
        {
            t.PartnerPassword = TransactionExtension
                .EncodePartnerPassword(t.PartnerPassword); return t;
        }).ToList();

        Transactions.ForEach(p =>
        {
            p.Sig = TransactionExtension.GenSigMsg(
                 Transactions, p.PartnerKey);
        });

        Transactions.Select(t =>
        {
            t.Sig = TransactionExtension
                .EncodeSHA265HashSig(t.Sig); return t;
        }).ToList();
        
    }

    public static List<Transaction> GetAll() => Transactions;

    public static Transaction? Get(string partnerRefNo) => Transactions.FirstOrDefault(t => t.PartnerRefNo == partnerRefNo);

    public static void Update(Transaction transaction)
    {
        var index = Transactions.FindIndex(t => t.PartnerRefNo == transaction.PartnerRefNo);

        if (index == -1)
            return;

        Transactions[index] = transaction;
    }
}