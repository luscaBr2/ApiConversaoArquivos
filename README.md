# 📄 API de Conversão de Arquivos para JSON

API REST desenvolvida em C# (.NET 10.0) que converte diversos formatos de arquivo para JSON de forma rápida e eficiente.

## 🌐 URL da API

```
http://apiconversaoarquivos-luscabr2.runasp.net
```

## 🚀 Formatos Suportados

- **PDF** (.pdf) - Extração de texto e metadados
- **Excel** (.xlsx, .xls, .xlsm) - Planilhas com múltiplas abas
- **CSV** (.csv) - Arquivos de valores separados por vírgula
- **SQL** (.sql) - Execução de queries e retorno de resultados

## 📋 Índice

- [Instalação](#instalação)
- [Endpoints](#endpoints)
- [Exemplos de Uso](#exemplos-de-uso)
- [Exemplos de Resposta](#exemplos-de-resposta)
- [Códigos de Status](#códigos-de-status)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Frontend](#frontend)

---

## 🔧 Instalação Local

### Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 ou VS Code

### Pacotes NuGet Utilizados

```bash
dotnet add package iTextSharp
dotnet add package ExcelDataReader
dotnet add package ExcelDataReader.DataSet
dotnet add package System.Text.Encoding.CodePages
dotnet add package CsvHelper
dotnet add package Newtonsoft.Json
dotnet add package Microsoft.Data.SqlClient
dotnet add package Swashbuckle.AspNetCore
```

### Executar Localmente

```bash
# Clone o repositório
git clone https://github.com/luscaBr2/ApiConversaoArquivos.git

# Entre na pasta do projeto
cd ApiConversaoArquivos

# Restaure as dependências
dotnet restore

# Execute o projeto
dotnet run
```

A API estará disponível em `https://localhost:5214` ou `http://localhost:7175`

---

## 📡 Endpoints

### POST /api/convert/

Endpoint único que converte qualquer arquivo suportado para JSON.

**URL Completa:**

```
http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/
```

**Método:** `POST`

**Content-Type:** `multipart/form-data`

**Parâmetros:**

| Parâmetro          | Tipo   | Obrigatório | Descrição                                                                 |
| ------------------ | ------ | ----------- | ------------------------------------------------------------------------- |
| `file`             | File   | Sim         | Arquivo a ser convertido                                                  |
| `connectionString` | String | Não\*       | Connection string do SQL Server (\*obrigatório apenas para arquivos .sql) |

---

## 💡 Exemplos de Uso

### 1. Converter CSV

**cURL:**

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/"
  -F "file=@produtos.csv"
```

**JavaScript (Fetch):**

```javascript
const formData = new FormData();
formData.append("file", fileInput.files[0]);

const response = await fetch(
    "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/",
    {
        method: "POST",
        body: formData,
    },
);

const result = await response.json();
console.log(result);
```

**C#:**

```csharp
using var client = new HttpClient();
using var form = new MultipartFormDataContent();
using var fileContent = new ByteArrayContent(File.ReadAllBytes("produtos.csv"));

fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
form.Add(fileContent, "file", "produtos.csv");

var response = await client.PostAsync(
    "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/",
    form
);

var result = await response.Content.ReadAsStringAsync();
```

### 2. Converter Excel (.xlsx ou .xls)

**cURL:**

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/"
  -F "file=@planilha.xlsx"
```

### 3. Converter PDF

**cURL:**

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/"
  -F "file=@documento.pdf"
```

### 4. Executar SQL

**cURL:**

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/"
  -F "file=@query.sql"
  -F "connectionString=Server=localhost;Database=mydb;User Id=sa;Password=MyPass123;"
```

---

## 📊 Exemplos de Resposta

### Sucesso - CSV

```json
{
    "success": true,
    "message": "Arquivo CSV convertido com sucesso para JSON",
    "data": [
        {
            "Código": "P001",
            "Produto": "Notebook Dell",
            "Categoria": "Eletrônicos",
            "Preço": "3500.00",
            "Estoque": "15"
        },
        {
            "Código": "P002",
            "Produto": "Mouse Logitech",
            "Categoria": "Periféricos",
            "Preço": "85.00",
            "Estoque": "50"
        }
    ],
    "error": null
}
```

### Sucesso - Excel

```json
{
  "success": true,
  "message": "Arquivo Excel convertido com sucesso para JSON",
  "data": {
    "fileName": "vendas.xlsx",
    "fileType": "Excel",
    "totalSheets": 2,
    "sheets": [
      {
        "sheetName": "Janeiro",
        "rowCount": 150,
        "data": [
          {
            "Data": "2024-01-01",
            "Produto": "Item A",
            "Valor": 1500.00
          }
        ]
      },
      {
        "sheetName": "Fevereiro",
        "rowCount": 180,
        "data": [...]
      }
    ]
  },
  "error": null
}
```

### Sucesso - PDF

```json
{
    "success": true,
    "message": "Arquivo PDF convertido com sucesso para JSON",
    "data": {
        "fileName": "relatorio.pdf",
        "fileType": "PDF",
        "totalPages": 3,
        "pages": [
            {
                "pageNumber": 1,
                "content": "Texto extraído da primeira página...",
                "hasContent": true
            },
            {
                "pageNumber": 2,
                "content": "Texto extraído da segunda página...",
                "hasContent": true
            }
        ],
        "fullText": "Todo o texto do PDF concatenado..."
    },
    "error": null
}
```

### Sucesso - SQL

```json
{
    "success": true,
    "message": "Arquivo SQL executado com sucesso",
    "data": {
        "fileName": "clientes.sql",
        "queryType": "SQL",
        "totalRecords": 5,
        "records": [
            {
                "ClienteID": 1,
                "Nome": "João Silva",
                "Email": "joao@email.com",
                "Cidade": "São Paulo"
            }
        ]
    },
    "error": null
}
```

### Erro - Arquivo não enviado

```json
{
    "success": false,
    "message": "Nenhum arquivo foi enviado",
    "data": null,
    "error": "File is required"
}
```

### Erro - Formato não suportado

```json
{
    "success": false,
    "message": "Formato de arquivo não suportado",
    "data": null,
    "error": "A extensão '.docx' não é suportada. Tipos aceitos: PDF (.pdf), Excel (.xlsx, .xls), CSV (.csv), SQL (.sql)"
}
```

### Erro - ConnectionString ausente (SQL)

```json
{
    "success": false,
    "message": "Para arquivos .sql, é necessário fornecer a connectionString",
    "data": null,
    "error": "ConnectionString is required for .sql files"
}
```

---

## 📡 Códigos de Status HTTP

| Código | Descrição                                             |
| ------ | ----------------------------------------------------- |
| `200`  | Sucesso - Arquivo convertido com sucesso              |
| `400`  | Bad Request - Arquivo inválido ou parâmetros faltando |
| `500`  | Internal Server Error - Erro ao processar o arquivo   |

---

## 🛠️ Tecnologias Utilizadas

### Backend

- **.NET 8** - Framework principal
- **Minimal API** - Arquitetura de endpoints
- **iTextSharp** - Processamento de PDFs
- **ExcelDataReader** - Leitura de arquivos Excel (.xls, .xlsx, .xlsm)
- **CsvHelper** - Processamento de arquivos CSV
- **Newtonsoft.Json** - Serialização JSON
- **Microsoft.Data.SqlClient** - Execução de queries SQL
- **Swashbuckle** - Documentação OpenAPI/Swagger

### Estrutura do Projeto

```
ApiConversaoArquivos/
├── Models/
│   └── ConversionResponse.cs
├── Services/
│   ├── Interfaces/
│   │   └── IFileConverterService.cs
│   └── Implementations/
│       ├── PdfConverterService.cs
│       ├── ExcelConverterService.cs
│       ├── CsvConverterService.cs
│       └── SqlConverterService.cs
├── Endpoints/
│   └── FileConverterEndpoints.cs
└── Program.cs
```

---

## 🎨 Frontend

Um frontend completo em **React + TypeScript + Vite** está disponível na pasta `/frontend`.

### Executar Frontend

```bash
cd frontend
npm install
npm run dev
```

O frontend estará disponível em `http://localhost:5173`

**Recursos do Frontend:**

- Upload de arquivos via drag-and-drop
- Validação de formatos
- Preview do JSON convertido
- Download do resultado
- Interface moderna e responsiva
- Suporte para todos os formatos da API

---

## 🔐 Segurança

- **CORS** configurado para aceitar requisições de qualquer origem
- **Antiforgery** desabilitado para facilitar integração
- **Validação** de tipos de arquivo no servidor
- **Tratamento de erros** robusto

> ⚠️ **Nota:** Para ambientes de produção, configure adequadamente as políticas de CORS e implemente autenticação/autorização conforme necessário.

---

## 🔄 Changelog

### v1.0.0 (2026-01-23)

- ✅ Conversão de PDF para JSON
- ✅ Conversão de Excel (.xlsx, .xls, .xlsm) para JSON
- ✅ Conversão de CSV para JSON
- ✅ Execução de queries SQL e conversão para JSON
- ✅ Endpoint único unificado
- ✅ Documentação Swagger/OpenAPI
- ✅ Frontend React + TypeScript

---

## 🎯 Roadmap

- [ ] Suporte para conversão de Word (.docx)
- [ ] Suporte para conversão de PowerPoint (.pptx)
- [ ] Autenticação JWT
- [ ] Rate limiting
- [ ] Compressão de respostas
- [ ] Cache de conversões
- [ ] Processamento assíncrono para arquivos grandes
- [ ] Webhook para notificação de conclusão
