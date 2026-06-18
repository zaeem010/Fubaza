using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Contracts.Serialization;

using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.DTO.Services;

using Google.Apis.Auth.OAuth2;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Fubaza.Application.Infrastructure.Services
{
    public class LookUpService : ILookUpService
    {
        private readonly ILogger<LookUpService> _logger;
        private readonly ILookUpRepository _repository;
        private readonly OpenAISettings _openAISettings;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly GeminiSettings _geminiSettings;
        public LookUpService(ILogger<LookUpService> logger,
            ILookUpRepository repository,
            IOptions<OpenAISettings> openAISettings,
            IJsonSerializer jsonSerializer,
            IOptions<GeminiSettings> geminiSettings
            )
        {
            _logger = logger;
            _repository = repository;
            _jsonSerializer = jsonSerializer;
            _openAISettings = openAISettings.Value;
            _geminiSettings = geminiSettings.Value;
        }

        public async Task<IResult<List<SportDto>>> GetSportsAsync()
        {
            try
            {
                var result = await _repository.GetSportsAsync();

                if (result != null)
                {
                    return await Result<List<SportDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the sport";

                _logger.LogError(message);
                return await Result<List<SportDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<SportDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<List<PlayingPositionDto>>> GetPlayingPositionsAsync(Guid sportId)
        {
            try
            {
                var result = await _repository.GetPlayingPositionsAsync(sportId);

                if (result != null)
                {
                    return await Result<List<PlayingPositionDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the sport";

                _logger.LogError(message);
                return await Result<List<PlayingPositionDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<PlayingPositionDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<List<DesignationDto>>> GetDesignationsAsync()
        {
            try
            {
                var result = await _repository.GetDesignationsAsync();

                if (result != null)
                {
                    return await Result<List<DesignationDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the designation";

                _logger.LogError(message);
                return await Result<List<DesignationDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<DesignationDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<List<TempleteTypeDTO>>> GetTempleteTypeAsync()
        {
            try
            {
                var values = Enum.GetValues(typeof(TempleteType))
                         .Cast<TempleteType>()
                         .Select(e => new TempleteTypeDTO
                         {
                             Id = (int)e,
                             Name = EnumExtensions.GetLocalizedEnum(e)
                         })
                         .ToList();

                if (values != null)
                {
                    return await Result<List<TempleteTypeDTO>>.SuccessAsync(values);
                }

                const string message = "Unable to get the Templete Type";

                _logger.LogError(message);
                return await Result<List<TempleteTypeDTO>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<TempleteTypeDTO>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<List<StrongFootDTO>>> GetStrongFootOptionsAsync()
        {
            try
            {
                var values = Enum.GetValues(typeof(StrongFoot))
                         .Cast<StrongFoot>()
                         .Select(e => new StrongFootDTO
                         {
                             Id = (int)e,
                             Name = EnumExtensions.GetLocalizedEnum(e)
                         })
                         .ToList();

                if (values != null)
                {
                    return await Result<List<StrongFootDTO>>.SuccessAsync(values);
                }

                const string message = "Unable to get the Strong Foot";

                _logger.LogError(message);
                return await Result<List<StrongFootDTO>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<StrongFootDTO>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<List<EventTypeDTO>>> GetEventTypeAsync(Guid sportId)
        {
            try
            {
                var result = await _repository.GetEventTypeAsync(sportId);

                if (result != null)
                {
                    return await Result<List<EventTypeDTO>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Event Type";

                _logger.LogError(message);
                return await Result<List<EventTypeDTO>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<EventTypeDTO>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<List<TempleteDto>>> GetTempletesAsync(TempleteRequest request)
        {
            try
            {
                var templetes = await _repository.GetTempletesAsync(request);

                if (templetes != null)
                {
                    return await Result<List<TempleteDto>>.SuccessAsync(templetes);
                }

                const string message = "Unable to get the Templete";

                _logger.LogError(message);
                return await Result<List<TempleteDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<TempleteDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<WritePostCaptionDTO>> WritePostCaptionAsync(WritePostCaptionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_openAISettings.BaseUrl))
                {
                    const string baseUrlError = "OpenAI BaseUrl is not configured.";
                    _logger.LogError(baseUrlError);
                    return await Result<WritePostCaptionDTO>.FailAsync(baseUrlError);
                }

                using var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(_openAISettings.BaseUrl)
                };
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _openAISettings.ApiKey);

                // Backend adds random suffix to make prompt unique
                string uniqueSuffix = $" [style_id: {Guid.NewGuid()}]";
                string enhancedPrompt = request.Prompt + uniqueSuffix;

                var requestPayload = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                new
                {
                    role = "system",
                    content = "You are a social media copywriter. When a user provides a caption or message, correct the grammar, enhance the style, and rewrite it as an engaging social media post. Return only the improved versions. Do not explain or label them. Vary the phrasing, sentence structure, and word choice in each response, even for similar inputs."
                },
                new
                {
                    role = "user",
                    content = enhancedPrompt
                }
            },
                    max_tokens = 1000,
                    temperature = 1.0,
                    top_p = 0.95
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestPayload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("chat/completions", content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    const string errorMsg = "Failed to generate post description via OpenAI.";
                    _logger.LogError("{Message}: {Response}", errorMsg, json);
                    return await Result<WritePostCaptionDTO>.FailAsync(errorMsg);
                }

                using var doc = JsonDocument.Parse(json);
                var message = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                var postDescription = new WritePostCaptionDTO
                {
                    ResponseText = message?.Trim() ?? string.Empty
                };

                return await Result<WritePostCaptionDTO>.SuccessAsync(postDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GeneratePostDescriptionAsync: {Message}", ex.Message);
                return await Result<WritePostCaptionDTO>.FailAsync("An unexpected error occurred while generating post description.");
            }
        }

        public async Task<IResult<AIImangeEnhancementDTO>> AIImangeEnhancementAsync(TempleteGenerationRequest generationRequest)
        {
            try
            {
                // --- Validate inputs ---
                if (generationRequest.Templete is null)
                    return await Result<AIImangeEnhancementDTO>.FailAsync("Template and at least one image are required.");
                if (string.IsNullOrWhiteSpace(_geminiSettings.CredentialJson))
                    return await Result<AIImangeEnhancementDTO>.FailAsync("CredentialsJson is required.");

                // --- Build GoogleCredential from JSON string (or base64) ---
                GoogleCredential cred;
                try
                {
                    using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(_geminiSettings.CredentialJson));
                    cred = GoogleCredential.FromStream(jsonStream)
                           .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                }
                catch
                {
                    var bytes = Convert.FromBase64String(_geminiSettings.CredentialJson);
                    using var b64Stream = new MemoryStream(bytes);
                    cred = GoogleCredential.FromStream(b64Stream)
                           .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                }
                string token = await cred.UnderlyingCredential.GetAccessTokenForRequestAsync();

                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };

                using var http = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // --- Convert IFormFile -> Base64 ---
                static async Task<string> ToBase64(IFormFile f)
                {
                    using var ms = new MemoryStream();
                    await f.CopyToAsync(ms);
                    return Convert.ToBase64String(ms.ToArray());
                }

                var userParts = new List<object>();

                // Prompt always first
                if (!string.IsNullOrWhiteSpace(generationRequest.Prompt))
                    userParts.Add(new { text = generationRequest.Prompt });

                string templateBase64 = await ToBase64(generationRequest.Templete);
                userParts.Add(new
                {
                    inlineData = new
                    {
                        mimeType = generationRequest.Templete.ContentType ?? "image/jpeg",
                        data = templateBase64
                    }
                });

                if (generationRequest.Imgaes is { Count: > 0 })
                {
                    foreach (var img in generationRequest.Imgaes)
                    {
                        string imgBase64 = await ToBase64(img);
                        userParts.Add(new
                        {
                            inlineData = new
                            {
                                mimeType = img.ContentType ?? "image/jpeg",
                                data = imgBase64
                            }
                        });
                    }
                }

                string systemInstruction = @"
                            You are an image editing assistant.  
                            Your job is to take a sports template image as input and update it according to the user’s instructions.  
                            Always keep the original design, layout, and style of the template intact.  
                            Only change the specified elements such as:
                          - Team logos
                          - Event name
                          - Match details
                           - Colors (if requested)
                           If the user provides multiple logos or text replacements, place them in the correct positions within the template 
                           (e.g., left side logo, right side logo, headline text).  
                            Do not generate new layouts; only edit the given template.  
                            Always return the updated sports poster as the output image. "; 


                // --- Build request payload (use non-streaming endpoint) ---
                var body = new
                {
                    systemInstruction = new
                    {
                        role = "system",
                        parts = new object[]
                        {
                          new { text = systemInstruction }
                        }
                    },
                    contents = new[]
                    {
                         new { role = "user", parts = userParts }
                    },
                    generationConfig = new
                    {
                        maxOutputTokens = 8192, // sensible upper bound
                        temperature = 1.0,
                        topP = 0.95,
                        responseModalities = new[] { "TEXT", "IMAGE" }
                    },
                    safetySettings = new[]
                    {
                           new { category = "HARM_CATEGORY_HATE_SPEECH",       threshold = "OFF" },
                           new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "OFF" },
                           new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "OFF" },
                           new { category = "HARM_CATEGORY_HARASSMENT",        threshold = "OFF" }
                    }
                };

                var url = $"https://aiplatform.googleapis.com/v1/projects/{_geminiSettings.ProjectId}/locations/{_geminiSettings.Location}/publishers/google/models/{_geminiSettings.ModelName}:generateContent";
                var json = JsonSerializer.Serialize(body);
                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Version = System.Net.HttpVersion.Version11, // avoid H2 quirks
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                using var resp = await http.SendAsync(req);
                var respText = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return await Result<AIImangeEnhancementDTO>.FailAsync($"Vertex error {(int)resp.StatusCode}: {respText}");

                var resposnse = _jsonSerializer.Deserialize<GeminiResponse>(respText);

                string enhancedImage = resposnse?.Candidates?
               .SelectMany(c => c.Content?.Parts ?? new List<Part>())
               .FirstOrDefault(p => p.InlineData?.Data is not null)
                ?.InlineData?.Data ?? string.Empty;

                if (string.IsNullOrEmpty(enhancedImage))
                {
                    enhancedImage = templateBase64; // fallback
                }

                var imageEnhancement = new AIImangeEnhancementDTO
                {
                    ImageBase64 = enhancedImage
                };

                return await Result<AIImangeEnhancementDTO>.SuccessAsync(imageEnhancement);
            }
            catch (Exception ex)
            {
                // include inner exception details for this class of errors
                var msg = ex.InnerException is null ? ex.Message : $"{ex.Message} :: {ex.InnerException.Message}";
                return await Result<AIImangeEnhancementDTO>.FailAsync(msg);
            }
        }

        public async Task<IResult<List<ThrowingHandDTO>>> GetThrowingHandOptionsAsync()
        {
            try
            {
                var values = Enum.GetValues(typeof(ThrowingHand))
                         .Cast<ThrowingHand>()
                         .Select(e => new ThrowingHandDTO
                         {
                             Id = (int)e,
                             Name = EnumExtensions.GetLocalizedEnum(e)
                         })
                         .ToList();

                if (values != null)
                {
                    return await Result<List<ThrowingHandDTO>>.SuccessAsync(values);
                }

                const string message = "Unable to get the Throwing Hand options";

                _logger.LogError(message);
                return await Result<List<ThrowingHandDTO>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<ThrowingHandDTO>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<List<CompetitionTypeDto>>> GetCompetitionTypesAsync()
        {
            try
            {
                var values = Enum.GetValues(typeof(CompetitionType))
                         .Cast<CompetitionType>()
                         .Select(e => new CompetitionTypeDto
                         {
                             Id = (int)e,
                             Name = EnumExtensions.GetLocalizedEnum(e)
                         })
                         .ToList();

                if (values != null)
                {
                    return await Result<List<CompetitionTypeDto>>.SuccessAsync(values);
                }

                const string message = "Unable to get the Competition Types";

                _logger.LogError(message);
                return await Result<List<CompetitionTypeDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<CompetitionTypeDto>>.FailAsync(e.Message);
            }
        }
    }

}





