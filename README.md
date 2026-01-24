# 📄 API de Conversão de Arquivos para JSON

API REST desenvolvida em C# (.NET 10.0) que converte diversos formatos de arquivo para JSON de forma rápida e eficiente.

## 🌐 URL da API

```
http://apiconversaoarquivos-luscabr2.runasp.net
```

## 🚀 Formatos Suportados

| Formato   | Extensões                | Descrição                                       |
| --------- | ------------------------ | ----------------------------------------------- |
| **PDF**   | `.pdf`                   | Extração de texto por página e texto completo   |
| **Excel** | `.xlsx`, `.xls`, `.xlsm` | Planilhas com múltiplas abas e dados tabulares  |
| **CSV**   | `.csv`                   | Arquivos de valores separados por vírgula       |
| **Word**  | `.docx`                  | Documentos com parágrafos, tabelas e formatação |
| **XML**   | `.xml`                   | Arquivos XML convertidos preservando estrutura  |
| **Text**  | `.txt`                   | Arquivos de texto simples linha por linha       |
| **Log**   | `.log`                   | Arquivos de log com detecção de níveis e erros  |

---

## 📡 Endpoint Principal

### POST /api/convert/

Endpoint único que converte qualquer arquivo suportado para JSON.

**URL Completa:**

```
http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/
```

**Método:** `POST`

**Content-Type:** `multipart/form-data`

**Parâmetros:**

| Parâmetro | Tipo | Obrigatório | Descrição                |
| --------- | ---- | ----------- | ------------------------ |
| `file`    | File | Sim         | Arquivo a ser convertido |

**Códigos de Resposta:**

| Código | Descrição                                               |
| ------ | ------------------------------------------------------- |
| `200`  | Sucesso - Arquivo convertido                            |
| `400`  | Bad Request - Arquivo inválido ou formato não suportado |
| `500`  | Internal Server Error - Erro ao processar               |

---

## 📋 Documentação Detalhada por Formato

### 1. PDF (.pdf)

**Descrição:** Extrai texto de cada página e retorna o texto completo do documento.

#### 📄 Exemplo de Arquivo: `relatorio.pdf`

**Conteúdo:**

```
Página 1:
Relatório de Vendas - 2024
Total de vendas: R$ 150.000,00

Página 2:
Produtos mais vendidos:
1. Notebook - 45 unidades
2. Mouse - 120 unidades
```

#### 💻 Request (cURL):

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@relatorio.pdf"
```

#### ✅ Response (JSON):

```json
{
    "success": true,
    "message": "Arquivo PDF convertido com sucesso para JSON",
    "data": {
        "fileName": "relatorio.pdf",
        "fileType": "PDF",
        "totalPages": 2,
        "pages": [
            {
                "pageNumber": 1,
                "content": "Relatório de Vendas - 2024\nTotal de vendas: R$ 150.000,00",
                "hasContent": true
            },
            {
                "pageNumber": 2,
                "content": "Produtos mais vendidos:\n1. Notebook - 45 unidades\n2. Mouse - 120 unidades",
                "hasContent": true
            }
        ],
        "fullText": "Relatório de Vendas - 2024\nTotal de vendas: R$ 150.000,00\n\nProdutos mais vendidos:\n1. Notebook - 45 unidades\n2. Mouse - 120 unidades"
    },
    "error": null
}
```

#### 📊 Campos Retornados:

| Campo                | Tipo    | Descrição                         |
| -------------------- | ------- | --------------------------------- |
| `fileName`           | string  | Nome do arquivo                   |
| `fileType`           | string  | "PDF"                             |
| `totalPages`         | number  | Número total de páginas           |
| `pages`              | array   | Array com conteúdo de cada página |
| `pages[].pageNumber` | number  | Número da página                  |
| `pages[].content`    | string  | Texto extraído da página          |
| `pages[].hasContent` | boolean | Se a página tem conteúdo          |
| `fullText`           | string  | Texto completo do documento       |

---

### 2. Excel (.xlsx, .xls, .xlsm)

**Descrição:** Extrai dados de todas as planilhas do arquivo Excel.

#### 📄 Exemplo de Arquivo: `vendas.xlsx`

**Planilha "Janeiro":**

| Produto  | Quantidade | Valor |
| -------- | ---------- | ----- |
| Notebook | 10         | 35000 |
| Mouse    | 50         | 2500  |

**Planilha "Fevereiro":**

| Produto | Quantidade | Valor |
| ------- | ---------- | ----- |
| Teclado | 30         | 4500  |
| Monitor | 15         | 7500  |

#### 💻 Request (cURL):

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@vendas.xlsx"
```

#### ✅ Response (JSON):

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
                "rowCount": 2,
                "data": [
                    {
                        "Produto": "Notebook",
                        "Quantidade": 10,
                        "Valor": 35000
                    },
                    {
                        "Produto": "Mouse",
                        "Quantidade": 50,
                        "Valor": 2500
                    }
                ]
            },
            {
                "sheetName": "Fevereiro",
                "rowCount": 2,
                "data": [
                    {
                        "Produto": "Teclado",
                        "Quantidade": 30,
                        "Valor": 4500
                    },
                    {
                        "Produto": "Monitor",
                        "Quantidade": 15,
                        "Valor": 7500
                    }
                ]
            }
        ]
    },
    "error": null
}
```

#### 📊 Campos Retornados:

| Campo                | Tipo   | Descrição                                        |
| -------------------- | ------ | ------------------------------------------------ |
| `fileName`           | string | Nome do arquivo                                  |
| `fileType`           | string | "Excel"                                          |
| `totalSheets`        | number | Número de planilhas                              |
| `sheets`             | array  | Array com dados de cada planilha                 |
| `sheets[].sheetName` | string | Nome da planilha                                 |
| `sheets[].rowCount`  | number | Número de linhas de dados                        |
| `sheets[].data`      | array  | Array de objetos (primeira linha como cabeçalho) |

---

### 3. CSV (.csv)

**Descrição:** Converte arquivo CSV em array de objetos JSON.

#### 📄 Exemplo de Arquivo: `clientes.csv`

```csv
Nome,Email,Cidade,Idade
João Silva,joao@email.com,São Paulo,30
Maria Santos,maria@email.com,Rio de Janeiro,25
Pedro Oliveira,pedro@email.com,Belo Horizonte,35
```

#### 💻 Request (cURL):

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@clientes.csv"
```

#### ✅ Response (JSON):

```json
{
    "success": true,
    "message": "Arquivo CSV convertido com sucesso para JSON",
    "data": [
        {
            "Nome": "João Silva",
            "Email": "joao@email.com",
            "Cidade": "São Paulo",
            "Idade": "30"
        },
        {
            "Nome": "Maria Santos",
            "Email": "maria@email.com",
            "Cidade": "Rio de Janeiro",
            "Idade": "25"
        },
        {
            "Nome": "Pedro Oliveira",
            "Email": "pedro@email.com",
            "Cidade": "Belo Horizonte",
            "Idade": "35"
        }
    ],
    "error": null
}
```

#### 📊 Campos Retornados:

| Campo    | Tipo   | Descrição                                       |
| -------- | ------ | ----------------------------------------------- |
| `data`   | array  | Array de objetos com os dados do CSV            |
| `data[]` | object | Cada linha como objeto (colunas = propriedades) |

**Observação:** A primeira linha do CSV é usada como cabeçalho (nomes das propriedades).

---

### 4. Word (.docx)

**Descrição:** Extrai parágrafos, tabelas e formatação de documentos Word.

#### 📄 Exemplo de Arquivo: `documento.docx`

**Conteúdo:**

```
Título Principal (Heading1, Negrito)

Este é um parágrafo normal com texto explicativo.

Subtítulo (Heading2, Negrito)

Outro parágrafo com informações importantes.

Tabela:
| Nome     | Cargo    | Salário |
|----------|----------|---------|
| Ana      | Gerente  | 8000    |
| Carlos   | Analista | 5000    |
```

#### 💻 Request (cURL):

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@documento.docx"
```

#### ✅ Response (JSON):

```json
{
    "success": true,
    "message": "Arquivo Word convertido com sucesso para JSON",
    "data": {
        "fileName": "documento.docx",
        "fileType": "Word",
        "totalParagraphs": 4,
        "totalTables": 1,
        "paragraphs": [
            {
                "index": 0,
                "text": "Título Principal",
                "style": "Heading1",
                "isHeading": true,
                "isBold": true,
                "isItalic": false
            },
            {
                "index": 1,
                "text": "Este é um parágrafo normal com texto explicativo.",
                "style": "Normal",
                "isHeading": false,
                "isBold": false,
                "isItalic": false
            },
            {
                "index": 2,
                "text": "Subtítulo",
                "style": "Heading2",
                "isHeading": true,
                "isBold": true,
                "isItalic": false
            },
            {
                "index": 3,
                "text": "Outro parágrafo com informações importantes.",
                "style": "Normal",
                "isHeading": false,
                "isBold": false,
                "isItalic": false
            }
        ],
        "tables": [
            {
                "index": 0,
                "headers": ["Nome", "Cargo", "Salário"],
                "rowCount": 2,
                "columnCount": 3,
                "rows": [
                    ["Ana", "Gerente", "8000"],
                    ["Carlos", "Analista", "5000"]
                ]
            }
        ],
        "fullText": "Título Principal\nEste é um parágrafo normal com texto explicativo.\nSubtítulo\nOutro parágrafo com informações importantes."
    },
    "error": null
}
```

#### 📊 Campos Retornados:

| Campo                    | Tipo    | Descrição                               |
| ------------------------ | ------- | --------------------------------------- |
| `fileName`               | string  | Nome do arquivo                         |
| `fileType`               | string  | "Word"                                  |
| `totalParagraphs`        | number  | Número total de parágrafos              |
| `totalTables`            | number  | Número total de tabelas                 |
| `paragraphs`             | array   | Array com dados de cada parágrafo       |
| `paragraphs[].index`     | number  | Índice do parágrafo                     |
| `paragraphs[].text`      | string  | Texto do parágrafo                      |
| `paragraphs[].style`     | string  | Estilo aplicado (Normal, Heading1, etc) |
| `paragraphs[].isHeading` | boolean | Se é um título                          |
| `paragraphs[].isBold`    | boolean | Se tem negrito                          |
| `paragraphs[].isItalic`  | boolean | Se tem itálico                          |
| `tables`                 | array   | Array com dados de cada tabela          |
| `tables[].index`         | number  | Índice da tabela                        |
| `tables[].headers`       | array   | Cabeçalhos da tabela (primeira linha)   |
| `tables[].rowCount`      | number  | Número de linhas de dados               |
| `tables[].columnCount`   | number  | Número de colunas                       |
| `tables[].rows`          | array   | Array com dados de cada linha           |
| `fullText`               | string  | Texto completo do documento             |

---

### 5. XML (.xml)

**Descrição:** Converte XML para JSON preservando a estrutura hierárquica.

#### 📄 Exemplo de Arquivo: `config.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <database>
    <host>localhost</host>
    <port>5432</port>
    <name>mydb</name>
  </database>
  <features>
    <feature enabled="true">authentication</feature>
    <feature enabled="false">logging</feature>
  </features>
</configuration>
```

#### 💻 Request (cURL):

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@config.xml"
```

#### ✅ Response (JSON):

```json
{
    "success": true,
    "message": "Arquivo XML convertido com sucesso para JSON",
    "data": {
        "fileName": "config.xml",
        "fileType": "XML",
        "rootElement": "configuration",
        "xmlData": {
            "configuration": {
                "database": {
                    "host": "localhost",
                    "port": "5432",
                    "name": "mydb"
                },
                "features": {
                    "feature": [
                        {
                            "@enabled": "true",
                            "#text": "authentication"
                        },
                        {
                            "@enabled": "false",
                            "#text": "logging"
                        }
                    ]
                }
            }
        },
        "rawXml": "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<configuration>\n  <database>\n    <host>localhost</host>\n    <port>5432</port>\n    <name>mydb</name>\n  </database>\n  <features>\n    <feature enabled=\"true\">authentication</feature>\n    <feature enabled=\"false\">logging</feature>\n  </features>\n</configuration>"
    },
    "error": null
}
```

#### 📊 Campos Retornados:

| Campo         | Tipo   | Descrição                          |
| ------------- | ------ | ---------------------------------- |
| `fileName`    | string | Nome do arquivo                    |
| `fileType`    | string | "XML"                              |
| `rootElement` | string | Nome do elemento raiz do XML       |
| `xmlData`     | object | Estrutura XML convertida para JSON |
| `rawXml`      | string | Conteúdo XML original como string  |

**Observação:** Atributos XML são convertidos com prefixo `@` e valores de texto com `#text`.

---

### 6. Text (.txt)

**Descrição:** Converte arquivo de texto em array de linhas com metadados.

#### 📄 Exemplo de Arquivo: `notas.txt`

```
Lista de Tarefas

1. Revisar documentação
2. Corrigir bugs
3. Implementar novas features

Status: Em andamento
```

#### 💻 Request (cURL):

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@notas.txt"
```

#### ✅ Response (JSON):

```json
{
    "success": true,
    "message": "Arquivo de texto convertido com sucesso para JSON",
    "data": {
        "fileName": "notas.txt",
        "fileType": "Text",
        "totalLines": 7,
        "lines": [
            {
                "lineNumber": 1,
                "content": "Lista de Tarefas",
                "length": 16,
                "isEmpty": false
            },
            {
                "lineNumber": 2,
                "content": "",
                "length": 0,
                "isEmpty": true
            },
            {
                "lineNumber": 3,
                "content": "1. Revisar documentação",
                "length": 23,
                "isEmpty": false
            },
            {
                "lineNumber": 4,
                "content": "2. Corrigir bugs",
                "length": 16,
                "isEmpty": false
            },
            {
                "lineNumber": 5,
                "content": "3. Implementar novas features",
                "length": 29,
                "isEmpty": false
            },
            {
                "lineNumber": 6,
                "content": "",
                "length": 0,
                "isEmpty": true
            },
            {
                "lineNumber": 7,
                "content": "Status: Em andamento",
                "length": 20,
                "isEmpty": false
            }
        ],
        "fullText": "Lista de Tarefas\n\n1. Revisar documentação\n2. Corrigir bugs\n3. Implementar novas features\n\nStatus: Em andamento\n"
    },
    "error": null
}
```

#### 📊 Campos Retornados:

| Campo                | Tipo    | Descrição                      |
| -------------------- | ------- | ------------------------------ |
| `fileName`           | string  | Nome do arquivo                |
| `fileType`           | string  | "Text"                         |
| `totalLines`         | number  | Número total de linhas         |
| `lines`              | array   | Array com dados de cada linha  |
| `lines[].lineNumber` | number  | Número da linha                |
| `lines[].content`    | string  | Conteúdo da linha              |
| `lines[].length`     | number  | Tamanho da linha em caracteres |
| `lines[].isEmpty`    | boolean | Se a linha está vazia          |
| `fullText`           | string  | Texto completo do arquivo      |

---

### 7. Log (.log)

**Descrição:** Analisa arquivos de log com detecção automática de timestamps, níveis e erros.

#### 📄 Exemplo de Arquivo: `application.log`

```
[2024-01-24T10:30:00] INFO Application started
[2024-01-24T10:30:05] DEBUG Loading configuration
[2024-01-24T10:30:10] INFO Database connection established
[2024-01-24T10:35:22] WARN Cache miss for key: user_1234
[2024-01-24T10:40:15] ERROR Failed to process request: Connection timeout
[2024-01-24T10:40:16] ERROR Stack trace: at DatabaseService.Query()
[2024-01-24T10:45:00] INFO Request processed successfully
```

#### 💻 Request (cURL):

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@application.log"
```

#### ✅ Response (JSON):

```json
{
    "success": true,
    "message": "Arquivo de log convertido com sucesso para JSON",
    "data": {
        "fileName": "application.log",
        "fileType": "Log",
        "totalLines": 7,
        "errorCount": 2,
        "logLevelStats": {
            "INFO": 3,
            "DEBUG": 1,
            "WARN": 1,
            "ERROR": 2
        },
        "entries": [
            {
                "lineNumber": 1,
                "content": "[2024-01-24T10:30:00] INFO Application started",
                "length": 43,
                "timestamp": "2024-01-24T10:30:00",
                "logLevel": "INFO",
                "isError": false,
                "isEmpty": false
            },
            {
                "lineNumber": 2,
                "content": "[2024-01-24T10:30:05] DEBUG Loading configuration",
                "length": 51,
                "timestamp": "2024-01-24T10:30:05",
                "logLevel": "DEBUG",
                "isError": false,
                "isEmpty": false
            },
            {
                "lineNumber": 3,
                "content": "[2024-01-24T10:30:10] INFO Database connection established",
                "length": 59,
                "timestamp": "2024-01-24T10:30:10",
                "logLevel": "INFO",
                "isError": false,
                "isEmpty": false
            },
            {
                "lineNumber": 4,
                "content": "[2024-01-24T10:35:22] WARN Cache miss for key: user_1234",
                "length": 58,
                "timestamp": "2024-01-24T10:35:22",
                "logLevel": "WARN",
                "isError": false,
                "isEmpty": false
            },
            {
                "lineNumber": 5,
                "content": "[2024-01-24T10:40:15] ERROR Failed to process request: Connection timeout",
                "length": 75,
                "timestamp": "2024-01-24T10:40:15",
                "logLevel": "ERROR",
                "isError": true,
                "isEmpty": false
            },
            {
                "lineNumber": 6,
                "content": "[2024-01-24T10:40:16] ERROR Stack trace: at DatabaseService.Query()",
                "length": 73,
                "timestamp": "2024-01-24T10:40:16",
                "logLevel": "ERROR",
                "isError": true,
                "isEmpty": false
            },
            {
                "lineNumber": 7,
                "content": "[2024-01-24T10:45:00] INFO Request processed successfully",
                "length": 58,
                "timestamp": "2024-01-24T10:45:00",
                "logLevel": "INFO",
                "isError": false,
                "isEmpty": false
            }
        ],
        "fullText": "[2024-01-24T10:30:00] INFO Application started\n[2024-01-24T10:30:05] DEBUG Loading configuration\n[2024-01-24T10:30:10] INFO Database connection established\n[2024-01-24T10:35:22] WARN Cache miss for key: user_1234\n[2024-01-24T10:40:15] ERROR Failed to process request: Connection timeout\n[2024-01-24T10:40:16] ERROR Stack trace: at DatabaseService.Query()\n[2024-01-24T10:45:00] INFO Request processed successfully\n"
    },
    "error": null
}
```

#### 📊 Campos Retornados:

| Campo                  | Tipo    | Descrição                                    |
| ---------------------- | ------- | -------------------------------------------- |
| `fileName`             | string  | Nome do arquivo                              |
| `fileType`             | string  | "Log"                                        |
| `totalLines`           | number  | Número total de linhas                       |
| `errorCount`           | number  | Número de linhas com erros                   |
| `logLevelStats`        | object  | Estatísticas de níveis de log                |
| `entries`              | array   | Array com dados de cada linha                |
| `entries[].lineNumber` | number  | Número da linha                              |
| `entries[].content`    | string  | Conteúdo da linha                            |
| `entries[].length`     | number  | Tamanho da linha                             |
| `entries[].timestamp`  | string  | Timestamp extraído (se encontrado)           |
| `entries[].logLevel`   | string  | Nível de log (INFO, DEBUG, WARN, ERROR, etc) |
| `entries[].isError`    | boolean | Se a linha contém erro                       |
| `entries[].isEmpty`    | boolean | Se a linha está vazia                        |
| `fullText`             | string  | Texto completo do log                        |

**Níveis de Log Detectados:** DEBUG, INFO, WARN, WARNING, ERROR, FATAL, TRACE, CRITICAL

---

## ❌ Respostas de Erro

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
    "error": "A extensão '.zip' não é suportada. Tipos aceitos: PDF (.pdf), Excel (.xlsx, .xls, .xlsm), CSV (.csv), Word (.docx), XML (.xml), Text (.txt), Log (.log)"
}
```

### Erro - Processamento

```json
{
    "success": false,
    "message": "Erro ao processar arquivo",
    "data": null,
    "error": "Mensagem de erro específica"
}
```

---

## 💡 Exemplos de Uso em Diferentes Linguagens

### JavaScript (Fetch)

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
console.log(result.data);
```

### Python (requests)

```python
import requests

url = 'http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/'
files = {'file': open('documento.pdf', 'rb')}

response = requests.post(url, files=files)
data = response.json()
print(data['data'])
```

### C# (HttpClient)

```csharp
using var client = new HttpClient();
using var form = new MultipartFormDataContent();
using var fileContent = new ByteArrayContent(File.ReadAllBytes("arquivo.xlsx"));

fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
form.Add(fileContent, "file", "arquivo.xlsx");

var response = await client.PostAsync(
    "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/",
    form
);

var result = await response.Content.ReadAsStringAsync();
```

### PHP

```php
$curl = curl_init();

$file = new CURLFile('documento.pdf', 'application/pdf', 'documento.pdf');

curl_setopt_array($curl, [
    CURLOPT_URL => 'http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/',
    CURLOPT_RETURNTRANSFER => true,
    CURLOPT_POST => true,
    CURLOPT_POSTFIELDS => ['file' => $file]
]);

$response = curl_exec($curl);
$data = json_decode($response, true);

curl_close($curl);
print_r($data['data']);
```

### cURL

```bash
curl -X POST "http://apiconversaoarquivos-luscabr2.runasp.net/api/convert/" \
  -F "file=@/caminho/para/arquivo.csv"
```

---

## 🛠️ Tecnologias Utilizadas

| Tecnologia             | Versão   | Uso                       |
| ---------------------- | -------- | ------------------------- |
| .NET                   | 10.0     | Framework principal       |
| iTextSharp             | 5.5.13.3 | Processamento de PDFs     |
| ExcelDataReader        | 3.7.0    | Leitura de arquivos Excel |
| CsvHelper              | 30.0.1   | Processamento de CSV      |
| DocumentFormat.OpenXml | 3.0.0    | Processamento de Word     |
| System.Xml.Linq        | Built-in | Processamento de XML      |
| Newtonsoft.Json        | 13.0.3   | Serialização JSON         |
| Swashbuckle.AspNetCore | 6.5.0    | Documentação Swagger      |

---

## 📊 Limites e Características

| Característica                | Valor/Status                     |
| ----------------------------- | -------------------------------- |
| **Tamanho máximo de arquivo** | 100 MB                           |
| **Encoding padrão**           | UTF-8 (com detecção automática)  |
| **CORS**                      | Habilitado para todas as origens |
| **Rate Limiting**             | Não implementado                 |
| **Autenticação**              | Não requerida                    |
| **Cache**                     | Não implementado                 |
| **Timeout**                   | 30 segundos                      |

---

## 🔒 Segurança e Privacidade

- ✅ API pública sem autenticação
- ✅ CORS configurado para aceitar requisições de qualquer origem
- ✅ Validação de tipos de arquivo no servidor
- ✅ Tratamento robusto de erros
- ✅ Logs de requisições (apenas para debug)
- ⚠️ Arquivos não são armazenados após processamento
- ⚠️ Não há criptografia de dados em trânsito (HTTP)

> ⚠️ **IMPORTANTE:** Esta é uma API pública. Não envie arquivos com informações sensíveis, confidenciais ou protegidas por direitos autorais.

---

## 🐛 Solução de Problemas

### Erro de CORS

**Problema:** Erro de CORS ao fazer requisições do navegador.  
**Solução:** Verifique se está usando o método POST e Content-Type correto (multipart/form-data).

### Arquivo muito grande

**Problema:** Erro ao enviar arquivo grande.  
**Solução:** Verifique se o arquivo não excede 100 MB. Considere comprimir o arquivo antes do envio.

### Encoding incorreto

**Problema:** Caracteres estranhos na resposta.  
**Solução:** A API usa UTF-8 por padrão. Converta seu arquivo para UTF-8 antes do envio.

### XML malformado

**Problema:** Erro ao processar XML.  
**Solução:** Certifique-se de que o XML está bem formado e válido. Use um validador XML online.

### Excel sem dados

**Problema:** Planilha retorna vazia.  
**Solução:** Verifique se a primeira linha contém os cabeçalhos e se há dados nas linhas seguintes.

### PDF sem texto

**Problema:** PDF retorna vazio.  
**Solução:** PDFs baseados em imagens (scaneados) não têm texto extraível. Use OCR antes de converter.

---

## 📖 Documentação Interativa

Acesse a documentação interativa Swagger em:

```
http://apiconversaoarquivos-luscabr2.runasp.net/
```

A interface Swagger permite:

- ✅ Testar todos os endpoints diretamente no navegador
- ✅ Ver exemplos de requisições e respostas
- ✅ Baixar a especificação OpenAPI
- ✅ Copiar comandos cURL

---

## 🔄 Versionamento

**Versão atual:** 1.2.0

### Changelog

#### v1.2.0 (2026-01-24)

- ✅ Adicionado suporte para XML (.xml)
- ✅ Adicionado suporte para Text (.txt)
- ✅ Adicionado suporte para Log (.log)
- ✅ Detecção automática de níveis de log
- ✅ Estatísticas de log
- ✅ Extração de timestamps de logs

#### v1.1.0 (2026-01-24)

- ✅ Adicionado suporte para Word (.docx)
- ✅ Extração de parágrafos e tabelas
- ✅ Detecção de formatação (negrito, itálico, estilos)
- ❌ Removido suporte para SQL

#### v1.0.0 (2026-01-23)

- ✅ Suporte inicial para PDF, Excel e CSV
- ✅ Endpoint unificado
- ✅ Documentação Swagger
- ✅ CORS habilitado

---

## 🎯 Roadmap

### Próximas Versões

- [ ] Suporte para PowerPoint (.pptx)
- [ ] Suporte para imagens com OCR
- [ ] Suporte para arquivos compactados (.zip, .rar)
- [ ] Autenticação JWT
- [ ] Rate limiting
- [ ] Cache de conversões
- [ ] Processamento assíncrono para arquivos grandes
- [ ] Webhooks para notificação de conclusão

---

**Se este projeto foi útil para você, considere dar uma estrela no GitHub!**
