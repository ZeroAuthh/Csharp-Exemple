# Sistema de Autenticação ZeroAuth - Exemplo em C#

Aplicativo Windows Forms em C# que demonstra a integração com o sistema de autenticação ZeroAuth, permitindo login e registro de usuários através de chaves de licença ou credenciais de usuário.

## 📋 Sobre o Projeto

Este projeto é um exemplo de implementação do ZeroAuth em uma aplicação Windows Forms, oferecendo três métodos de autenticação:
- **Login com Chave de Licença**: Autenticação usando apenas uma chave de licença
- **Login com Usuário e Senha**: Autenticação tradicional com credenciais
- **Registro de Novo Usuário**: Criação de conta com usuário, senha e chave de licença

## 🛠️ Tecnologias Utilizadas

- **.NET Framework 4.7.2**
- **Windows Forms** para interface gráfica
- **System.Text.Json** para serialização JSON
- **Newtonsoft.Json** para manipulação de JSON
- **HttpClient** para comunicação com a API ZeroAuth

## 📦 Dependências

O projeto utiliza os seguintes pacotes NuGet:

- Microsoft.Bcl.AsyncInterfaces (9.0.4)
- Newtonsoft.Json (13.0.3)
- System.Buffers (4.5.1)
- System.CodeDom (9.0.4)
- System.IO.Pipelines (9.0.4)
- System.Memory (4.5.5)
- System.Numerics.Vectors (4.5.0)
- System.Runtime.CompilerServices.Unsafe (6.0.0)
- System.Text.Encodings.Web (9.0.4)
- System.Text.Json (9.0.4)
- System.Threading.Tasks.Extensions (4.5.4)
- System.ValueTuple (4.5.0)

## 🚀 Como Configurar

### Pré-requisitos

- Visual Studio 2017 ou superior
- .NET Framework 4.7.2 ou superior
- Conta no ZeroAuth com AppID e OwnerID configurados

### Instalação

1. Clone ou baixe este repositório
2. Abra o arquivo `auth.sln` no Visual Studio
3. Restaure os pacotes NuGet (o Visual Studio fará isso automaticamente ou execute `Update-Package -reinstall` no Package Manager Console)
4. Configure suas credenciais ZeroAuth no arquivo `Form1.cs`:

```csharp
public static ZeroAUTH ZeroAuth = new ZeroAUTH(
    Application: "seu-appid-aqui",      // Seu AppID
    OwnerID: "seu-ownerid-aqui"         // Seu OwnerID (Database)
);
```

5. Compile e execute o projeto (F5)

## 📁 Estrutura do Projeto

```
Form/
├── auth/
│   ├── Form1.cs              # Formulário principal de login/registro
│   ├── Form2.cs              # Formulário exibido após login com usuário
│   ├── Form3.cs              # Formulário exibido após login com chave
│   ├── ZeroAuth.cs           # Classe principal de autenticação
│   ├── Program.cs            # Ponto de entrada da aplicação
│   └── Properties/           # Configurações e recursos do projeto
├── packages/                 # Pacotes NuGet
└── auth.sln                  # Solução do Visual Studio
```

## 🔑 Funcionalidades

### ZeroAuth.cs

A classe `ZeroAUTH` fornece os seguintes métodos:

- **`Init()`**: Inicializa a conexão com a API ZeroAuth e verifica o status do AppID
- **`LoginWithKey(string key)`**: Realiza login usando apenas uma chave de licença
- **`LoginWithUser(string username, string password)`**: Realiza login com usuário e senha
- **`RegisterUserWithKey(string username, string password, string key)`**: Registra um novo usuário com chave de licença
- **`GetExpiration(string keyOrUsername, string format, bool isKey)`**: Obtém informações de expiração da licença
- **`CheckApiAvailability()`**: Verifica se a API está disponível

### Formulários

#### Form1 (Tela Principal)
- Campo para login com chave de licença
- Campos para login com usuário e senha
- Campos para registro de novo usuário (usuário, senha e chave)

#### Form2 (Após Login com Usuário)
- Exibe informações do usuário logado
- Mostra o tempo de expiração da licença

#### Form3 (Após Login com Chave)
- Exibe a chave utilizada
- Mostra o tempo de expiração da licença

## 🔒 Segurança

O sistema implementa as seguintes medidas de segurança:

- **HWID (Hardware ID)**: Identificação única do hardware para vinculação de licenças
- **Logs de Acesso**: Registro de todas as tentativas de login com informações de IP e hardware
- **Validação de Licença**: Verificação em tempo real com o servidor ZeroAuth
- **Tratamento de Erros**: Sistema robusto de tratamento de exceções e erros de conexão

## 📝 Logs de Erro

O sistema cria automaticamente um arquivo de log em `Logs/ErrorLogs.txt` sempre que ocorre um erro crítico. Os logs incluem:

- Data e hora do erro
- Mensagem de erro detalhada
- Informações de conexão

## ⚙️ Configuração da API

A API ZeroAuth está configurada para usar o endpoint:
```
https://api.zeroauth.cc
```

Certifique-se de que este endpoint está acessível e que suas credenciais (AppID e OwnerID) estão corretas.

## 🐛 Tratamento de Erros

O sistema trata os seguintes tipos de erros:

- **Erros de Conexão**: Timeout ou falha na comunicação com a API
- **Erros de Autenticação**: Credenciais inválidas ou chave expirada
- **Erros de JSON**: Respostas inválidas da API
- **Erros de Inicialização**: AppID inválido ou API offline

## 📄 Licença

Este projeto é um exemplo de implementação. Verifique a licença do ZeroAuth para uso comercial.

## 🤝 Contribuições

Este é um projeto de exemplo. Sinta-se livre para adaptá-lo às suas necessidades.

## ⚠️ Avisos

- **Nunca compartilhe suas credenciais** (AppID e OwnerID) publicamente
- **Use ofuscação de código** em produção para proteger sua aplicação
- **Implemente verificações de integridade** para prevenir modificações no código
- **Mantenha suas dependências atualizadas** para segurança

## 📞 Suporte

Para questões sobre o ZeroAuth, consulte a documentação oficial ou entre em contato com o suporte do ZeroAuth.

---

**Nota**: Este é um projeto de exemplo educacional. Adapte-o conforme necessário para seu uso específico.

