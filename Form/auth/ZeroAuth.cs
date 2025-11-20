using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class AuthResult
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public bool Success => StatusCode >= 200 && StatusCode < 300;
}

public class ZeroAUTH
{
    private readonly string application;
    private readonly string ownerID;
    private bool isInitialized = false;

    private static readonly HttpClient client = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };
    private static readonly string apiUrl = "https://api.zeroauth.cc";

    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            error("ZeroAuth não foi inicializado. Chame o método Init() primeiro antes de usar qualquer método da classe.");
            return;
        }
    }

    public ZeroAUTH(string Application, string OwnerID)
    {
        this.application = Application;
        this.ownerID = OwnerID;
    }

    public async Task Init()
    {
        try
        {
            HttpResponseMessage healthResponse = await client.GetAsync($"{apiUrl}/api/auth-api/health?appDatabase={Uri.EscapeDataString(ownerID)}");

            if (!healthResponse.IsSuccessStatusCode)
            {
                error($"ZeroAuth offline. Status: {healthResponse.StatusCode}. Verifique sua conexão.");
                return;
            }

            var content = new StringContent($"{{\"appid\":\"{EscapeJsonString(application)}\", \"appDatabase\":\"{EscapeJsonString(ownerID)}\"}}", Encoding.UTF8, "application/json");

            var statusResponse = await client.PostAsync($"{apiUrl}/api/auth-api/check-app-status", content);
            string responseBody = await statusResponse.Content.ReadAsStringAsync();

            if (statusResponse.IsSuccessStatusCode)
            {
                if (responseBody.Contains("AppID está ativo."))
                {
                    Console.WriteLine("AppID está ativo.");
                    isInitialized = true;
                }
            }
            else
            {
                string errorMessage = "Erro ao verificar status do AppID.";
                try
                {
                    using (JsonDocument jsonDoc = JsonDocument.Parse(responseBody))
                    {
                        if (jsonDoc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                        {
                            errorMessage = messageElement.GetString() ?? errorMessage;
                        }
                    }
                }
                catch
                {
                    errorMessage = responseBody.Length > 200 ? responseBody.Substring(0, 200) : responseBody;
                }
                error(errorMessage);
            }
        }
        catch (HttpRequestException ex)
        {
            error($"Erro de conexão ao inicializar ZeroAuth: {ex.Message}. Verifique sua conexão com a internet.");
        }
        catch (TaskCanceledException)
        {
            error("Timeout ao inicializar ZeroAuth. A requisição demorou muito para responder.");
        }
        catch (Exception ex)
        {
            error($"Erro ao inicializar ZeroAuth: {ex.Message}. Verifique sua conexão.");
        }
    }

    public async Task<string> GetExpiration(string keyOrUsername, string format, bool isKey = true)
    {
        if (!isInitialized)
        {
            error("ZeroAuth não foi inicializado. Chame o método Init() primeiro antes de usar qualquer método da classe.");
            return "Erro: ZeroAuth não inicializado.";
        }

        try
        {
            var content = new StringContent($"{{\"{(isKey ? "key" : "username")}\":\"{EscapeJsonString(keyOrUsername)}\", \"appid\":\"{EscapeJsonString(application)}\", \"appDatabase\":\"{EscapeJsonString(ownerID)}\", \"format\":\"{EscapeJsonString(format)}\"}}", Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiUrl}/api/auth-api/get-expiration", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using (JsonDocument jsonDoc = JsonDocument.Parse(responseBody))
                    {
                        if (jsonDoc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                        {
                            return messageElement.GetString() ?? "Erro: Resposta inválida da API.";
                        }
                    }
                }
                catch (JsonException)
                {
                    return "Erro: Resposta JSON inválida da API.";
                }
                return "Erro: Resposta inválida da API.";
            }
            else
            {
                return ParseErrorMessage(responseBody, "Erro ao obter a expiração.");
            }
        }
        catch (HttpRequestException ex)
        {
            return $"Erro de conexão: {ex.Message}. Verifique sua conexão com a internet.";
        }
        catch (TaskCanceledException)
        {
            return "Erro: Timeout ao obter expiração. A requisição demorou muito para responder.";
        }
        catch (JsonException)
        {
            return "Erro: Resposta JSON inválida da API.";
        }
        catch (Exception ex)
        {
            return $"Erro inesperado: {ex.Message}";
        }
    }





    public static void error(string message)
    {
        string folder = @"Logs", file = Path.Combine(folder, "ErrorLogs.txt");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (!File.Exists(file))
        {
            using (FileStream stream = File.Create(file))
            {
                File.AppendAllText(file, DateTime.Now + " > This is the start of your error logs file");
            }
        }

        File.AppendAllText(file, DateTime.Now + $" > {message}" + Environment.NewLine);

        Process.Start(new ProcessStartInfo("cmd.exe", $"/c start cmd /C \"color b && title Error && echo {message} && timeout /t 5\"")
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });
        Environment.Exit(0);
    }

    private static string GetHWID()
    {
        return $"{System.Security.Principal.WindowsIdentity.GetCurrent().User.Value}";
    }



    private async Task<string> GetIPInfo()
    {
        try
        {
            using (HttpClient ipClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) })
            {
                HttpResponseMessage response = await ipClient.GetAsync("http://ip-api.com/json/");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return "Unknown";
                }
            }
        }
        catch (HttpRequestException)
        {
            return "Unknown";
        }
        catch (TaskCanceledException)
        {
            return "Unknown";
        }
        catch (Exception)
        {
            return "Unknown";
        }
    }

    private async Task SendLogToAPI(string usernameOrKey, string hwid, string ipInfo, bool isKeyLogin, string appid, string appDatabase)
    {
        try
        {
            var jsonPayload = $"{{\"usernameOrKey\":\"{EscapeJsonString(usernameOrKey)}\", \"hwid\":\"{EscapeJsonString(hwid)}\", \"ipInfo\":\"{EscapeJsonString(ipInfo)}\", \"isKeyLogin\":{isKeyLogin.ToString().ToLower()}, \"computerUsername\":\"{EscapeJsonString(Environment.UserName)}\", \"appid\":\"{EscapeJsonString(appid)}\", \"appDatabase\":\"{EscapeJsonString(appDatabase)}\"}}";

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            Console.WriteLine($"Enviando para: {apiUrl}/api/auth-api/log-login");

            HttpResponseMessage response = await client.PostAsync($"{apiUrl}/api/auth-api/log-login", content);

            if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ao enviar log para API. Status: {response.StatusCode}, Resposta: {responseContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            error("Erro de requisição HTTP ao enviar log para API: " + ex.Message + "\nDetalhes: " + ex.ToString());
        }
        catch (TaskCanceledException ex)
        {
            error("Timeout ao enviar log para API: " + ex.Message + "\nDetalhes: " + ex.ToString());
        }
        catch (Exception ex)
        {
            error("Erro inesperado ao enviar log para API: " + ex.Message + "\nDetalhes: " + ex.ToString());
        }
    }

    private string EscapeJsonString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t")
                    .Replace("\b", "\\b")
                    .Replace("\f", "\\f");
    }

    private string ParseErrorMessage(string responseBody, string defaultMessage, int statusCode = 0)
    {
        try
        {
            using (JsonDocument jsonDoc = JsonDocument.Parse(responseBody))
            {
                // Se for status 404, procura especificamente pela mensagem
                if (statusCode == 404)
                {
                    if (jsonDoc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        string message = messageElement.GetString();
                        if (!string.IsNullOrEmpty(message))
                        {
                            return message;
                        }
                    }
                    // Se não encontrar "message", tenta outros campos comuns
                    if (jsonDoc.RootElement.TryGetProperty("error", out JsonElement errorElement))
                    {
                        string error = errorElement.GetString();
                        if (!string.IsNullOrEmpty(error))
                        {
                            return error;
                        }
                    }
                }
                else
                {
                    // Para outros status codes, procura normalmente pela mensagem
                    if (jsonDoc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        return messageElement.GetString() ?? defaultMessage;
                    }
                }
            }
        }
        catch
        {
            return responseBody.Length > 200 ? responseBody.Substring(0, 200) + "..." : responseBody;
        }
        return defaultMessage;
    }


    public async Task<bool> CheckApiAvailability()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync($"{apiUrl}/api/auth-api/health?appDatabase={Uri.EscapeDataString(ownerID)}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao verificar disponibilidade da API: {ex.Message}");
            return false;
        }
    }

    public async Task<AuthResult> LoginWithKey(string key)
    {
        if (!isInitialized)
        {
            error("ZeroAuth não foi inicializado. Chame o método Init() primeiro antes de usar qualquer método da classe.");
            return new AuthResult { StatusCode = 0, Message = "Erro: ZeroAuth não inicializado." };
        }

        try
        {
            var hwid = GetHWID();
            var content = new StringContent($"{{\"key\":\"{EscapeJsonString(key)}\", \"hwid\":\"{EscapeJsonString(hwid)}\", \"appid\":\"{EscapeJsonString(application)}\", \"appDatabase\":\"{EscapeJsonString(ownerID)}\"}}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{apiUrl}/api/auth-api/login", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Executa log em background sem bloquear a resposta
                _ = Task.Run(async () =>
                {
                    string ip = await GetIPInfo();
                    await SendLogToAPI(key, hwid, ip, true, application, ownerID);
                });
                return new AuthResult { StatusCode = (int)response.StatusCode, Message = "Login com a key bem-sucedido!" };
            }
            else
            {
                int statusCode = (int)response.StatusCode;
                return new AuthResult { StatusCode = statusCode, Message = ParseErrorMessage(responseBody, "Erro desconhecido ao fazer login com a key.", statusCode) };
            }
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult { StatusCode = 0, Message = $"Erro de conexão: {ex.Message}. Verifique sua conexão com a internet." };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult { StatusCode = 0, Message = "Erro: Timeout ao fazer login. A requisição demorou muito para responder." };
        }
        catch (JsonException)
        {
            return new AuthResult { StatusCode = 0, Message = "Erro: Resposta JSON inválida da API." };
        }
        catch (Exception ex)
        {
            return new AuthResult { StatusCode = 0, Message = $"Erro inesperado ao fazer login: {ex.Message}" };
        }
    }

    public async Task<AuthResult> LoginWithUser(string username, string password)
    {
        if (!isInitialized)
        {
            error("ZeroAuth não foi inicializado. Chame o método Init() primeiro antes de usar qualquer método da classe.");
            return new AuthResult { StatusCode = 0, Message = "Erro: ZeroAuth não inicializado." };
        }

        try
        {
            var hwid = GetHWID();
            var content = new StringContent($"{{\"username\":\"{EscapeJsonString(username)}\", \"password\":\"{EscapeJsonString(password)}\", \"hwid\":\"{EscapeJsonString(hwid)}\", \"appid\":\"{EscapeJsonString(application)}\", \"appDatabase\":\"{EscapeJsonString(ownerID)}\"}}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{apiUrl}/api/auth-api/user-login", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Executa log em background sem bloquear a resposta
                _ = Task.Run(async () =>
                {
                    string ip = await GetIPInfo();
                    await SendLogToAPI(username, hwid, ip, false, application, ownerID);
                });
                return new AuthResult { StatusCode = (int)response.StatusCode, Message = "Login com usuário bem-sucedido!" };
            }
            else
            {
                int statusCode = (int)response.StatusCode;
                return new AuthResult { StatusCode = statusCode, Message = ParseErrorMessage(responseBody, "Erro desconhecido ao fazer login com usuário.", statusCode) };
            }
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult { StatusCode = 0, Message = $"Erro de conexão: {ex.Message}. Verifique sua conexão com a internet." };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult { StatusCode = 0, Message = "Erro: Timeout ao fazer login. A requisição demorou muito para responder." };
        }
        catch (JsonException)
        {
            return new AuthResult { StatusCode = 0, Message = "Erro: Resposta JSON inválida da API." };
        }
        catch (Exception ex)
        {
            return new AuthResult { StatusCode = 0, Message = $"Erro inesperado ao fazer login: {ex.Message}" };
        }
    }

    public async Task<AuthResult> RegisterUserWithKey(string username, string password, string key)
    {
        if (!isInitialized)
        {
            error("ZeroAuth não foi inicializado. Chame o método Init() primeiro antes de usar qualquer método da classe.");
            return new AuthResult { StatusCode = 0, Message = "Erro: ZeroAuth não inicializado." };
        }

        try
        {
            var content = new StringContent($"{{\"username\":\"{EscapeJsonString(username)}\", \"password\":\"{EscapeJsonString(password)}\", \"key\":\"{EscapeJsonString(key)}\", \"appid\":\"{EscapeJsonString(application)}\", \"appDatabase\":\"{EscapeJsonString(ownerID)}\"}}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{apiUrl}/api/auth-api/register", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            int statusCode = (int)response.StatusCode;
            return new AuthResult
            {
                StatusCode = statusCode,
                Message = response.IsSuccessStatusCode ? "Usuário registrado com sucesso!" : ParseErrorMessage(responseBody, "Erro desconhecido ao registrar usuário.", statusCode)
            };
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult { StatusCode = 0, Message = $"Erro de conexão: {ex.Message}. Verifique sua conexão com a internet." };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult { StatusCode = 0, Message = "Erro: Timeout ao registrar usuário. A requisição demorou muito para responder." };
        }
        catch (JsonException)
        {
            return new AuthResult { StatusCode = 0, Message = "Erro: Resposta JSON inválida da API." };
        }
        catch (Exception ex)
        {
            return new AuthResult { StatusCode = 0, Message = $"Erro inesperado ao registrar usuário: {ex.Message}" };
        }
    }
}
