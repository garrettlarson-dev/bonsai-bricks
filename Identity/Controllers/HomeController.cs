using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Identity.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<AppUser> userManager;
        private readonly InferenceSession _session;
        private readonly ILogger<HomeController> _logger;
        private readonly AppIdentityDbContext _context;
        
        public HomeController(UserManager<AppUser> userMgr, ILogger<HomeController> logger, InferenceSession session, AppIdentityDbContext context)
        {
            _logger = logger;
            _session = session;
            userManager = userMgr;
            _context = context;
            
            try
            {
                _session = new InferenceSession("~/decision_tree_model.onnx");
                _logger.LogInformation("ONNX model loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading the ONNX model: {ex.Message}");
            }
        }
        
        

        [Authorize]
        //[Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index()
        {
            AppUser user = await userManager.GetUserAsync(HttpContext.User);
            string message = "Hello " + user.UserName;
            return View((object)message);
        }
        
        [HttpPost]
        public IActionResult Predict(int quantity)
        {
            // Dictionary mapping the numeric prediction to an animal type
            var class_type_dict = new Dictionary<int, string>
            {
                { 1, "valid" },
                { 2, "fraud" }
            };

            try
            {
                var input = new List<float> { quantity};
                var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
                };

                using (var results = _session.Run(inputs)) // makes the prediction with the inputs from the form (i.e. class_type 1-7)
                {
                    var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                    if (prediction != null && prediction.Length > 0)
                    {
                        // Use the prediction to get the animal type from the dictionary
                        var animalType = class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown");
                        ViewBag.Prediction = animalType;
                    }
                    else
                    {
                        ViewBag.Prediction = "Error: Unable to make a prediction.";
                    }
                }

                _logger.LogInformation("Prediction executed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during prediction: {ex.Message}");
                ViewBag.Prediction = "Error during prediction.";
            }

            return View("Index");
        }
        
        

        public async Task<IActionResult> Privacy()
        {
            return View(Privacy);
        }
    }
}