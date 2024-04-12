using Identity.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
namespace Identity.CustomTagHelpers;
public class PredictionService
{
    private readonly AppIdentityDbContext _context;
    private readonly InferenceSession _session;
    public PredictionService(AppIdentityDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var modelPath = Path.Combine(env.WebRootPath, "decision_tree_model.onnx");
        _session = new InferenceSession(modelPath);
    }

    public int PredictFraud(Order order, AppUser cust)
    {
        // Direct assignment for numeric values
        int time = order.Time ?? 0;
        int amount = order.Amount;
        

        
        Console.WriteLine("Order ID: " + order.TransactionId);
        Console.WriteLine("Customer ID: " + order.CustomerId);
        Console.WriteLine("Customer Age: " + cust.Age);
        
        //age
        if (cust == null)
        {
            throw new InvalidOperationException("Customer information is not available.");
        }

        int age = (int)Math.Round(cust.Age);

        
        int transaction_shipping_match = order.CountryOfTransaction == order.ShippingAddress ? 1 : 0;
        int residence_transaction_match = order.CountryOfTransaction == cust.CountryOfResidence ? 1 : 0;
        
        // One-hot encoding for categorical variables
        // Day of week
        int day_of_week_Mon = order.DayOfWeek == "Mon" ? 1 : 0;
        int day_of_week_Tue = order.DayOfWeek == "Tue" ? 1 : 0;
        int day_of_week_Wed = order.DayOfWeek == "Wed" ? 1 : 0;
        int day_of_week_Thu = order.DayOfWeek == "Thu" ? 1 : 0;
        int day_of_week_Sat = order.DayOfWeek == "Sat" ? 1 : 0;
        int day_of_week_Sun = order.DayOfWeek == "Sun" ? 1 : 0;
        
        // Entry mode
        int entry_mode_Tap = order.EntryMode == "Tap" ? 1 : 0;
        int entry_mode_PIN = order.EntryMode == "PIN" ? 1 : 0;

        // Type of transaction
        int type_of_transaction_POS = 0;
        int type_of_transaction_Online = 0;


        // Country of transaction
        int country_of_transaction_India = order.CountryOfTransaction == "India" ? 1 : 0;
        int country_of_transaction_Russia = order.CountryOfTransaction == "Russia" ? 1 : 0;
        int country_of_transaction_USA = order.CountryOfTransaction == "USA" ? 1 : 0;
        
        // Country of residence
        //int country_of_residence_China = cust.CountryOfResidence == "China" ? 1 : 0;
        int country_of_residence_India = cust.CountryOfResidence == "India" ? 1 : 0;
        int country_of_residence_Russia = cust.CountryOfResidence == "Russia" ? 1 : 0;
        int country_of_residence_USA = cust.CountryOfResidence == "USA" ? 1 : 0;
        int country_of_residence_United_Kingdom = cust.CountryOfResidence == "United Kingdom" ? 1 : 0;
        
        //shipping address
        int shipping_address_United_Kingdom = order.ShippingAddress == "United Kingdom" ? 1 : 0;
        int shipping_address_India = order.ShippingAddress == "India" ? 1 : 0;
        int shipping_address_Russia = order.ShippingAddress == "Russia" ? 1 : 0;
        int shipping_address_USA = order.ShippingAddress == "USA" ? 1 : 0;
        
        //bank
        int bank_HSBC = order.Bank == "HSBC" ? 1 : 0;
        int bank_Halifax = order.Bank == "Halifax" ? 1 : 0;
        int bank_Lloyds = order.Bank == "Lloyds" ? 1 : 0;
        int bank_Metro = order.Bank == "Metro" ? 1 : 0;
        int bank_Monzo = order.Bank == "Monzo" ? 1 : 0;
        int bank_RBS = order.Bank == "RBS" ? 1 : 0;
        
        //type of card
        int type_of_card_Visa = order.TypeOfCard == "Visa" ? 1 : 0;
        
        //gender
        int gender_M = cust.Gender == "M" ? 1 : 0;
        
        // Dictionary mapping the numeric prediction to an animal type
        var class_type_dict = new Dictionary<int, string>
        {
            { 0, "non-fraudulent" },
            { 1, "fraudulent" }
        };
        var input = new List<float> { 
                time,
                amount,
                age,
                transaction_shipping_match,
                residence_transaction_match,
                day_of_week_Mon,
                day_of_week_Sat,
                day_of_week_Sun,
                day_of_week_Thu,
                day_of_week_Tue,
                day_of_week_Wed,
                entry_mode_Tap,
                entry_mode_PIN,
                type_of_transaction_POS,
                country_of_residence_United_Kingdom,
                country_of_transaction_India,
                country_of_transaction_Russia,
                country_of_transaction_USA,
                shipping_address_United_Kingdom,
                shipping_address_India,
                shipping_address_Russia,
                shipping_address_USA,
                bank_HSBC,
                bank_Halifax,
                bank_Lloyds,
                bank_Metro,
                bank_Monzo,
                bank_RBS,
                type_of_card_Visa,
                gender_M,
                //country_of_residence_China,
                country_of_residence_India,
                country_of_residence_Russia,
                country_of_residence_USA
        };
        var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using (var results = _session.Run(inputs)) // makes the prediction with the inputs from the form
        {
            var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
            if (prediction != null && prediction.Length > 0)
            {
                // Use the prediction to get the animal type from the dictionary
                var fraudPrediction = class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown");
            }
            return (int)prediction[0];
        }
    }
}