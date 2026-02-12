# PraOndeFoi

PraOndeFoi é uma API abrangente de gerenciamento financeiro projetada para ajudar os usuários a rastrear transações, gerenciar orçamentos e obter insights sobre seus hábitos financeiros. Construída com tecnologias modernas, fornece um backend robusto para aplicações de finanças pessoais.

## Funcionalidades

- **Gerenciamento de Transações**: Criar, ler, atualizar e excluir transações financeiras com categorização e marcação.
- **Acompanhamento de Orçamentos**: Definir orçamentos mensais para categorias e monitorar gastos em relação aos limites.
- **Sistema de Tags**: Organizar transações com tags personalizadas para melhor filtragem e análise.
- **Transações Recorrentes**: Suporte para assinaturas e pagamentos recorrentes.
- **Metas Financeiras**: Definir e acompanhar o progresso em direção a objetivos financeiros.
- **Análises e Insights**: Gerar relatórios e insights usando previsões baseadas em médias móveis ponderadas.
- **Suporte a Múltiplas Contas**: Gerenciar múltiplas contas com isolamento adequado.
- **Autenticação**: Autenticação segura de usuários via Supabase.
- **Anexos de Arquivos**: Anexar recibos e documentos às transações.
- **Funcionalidade de Exportação**: Exportar dados para PDF e CSV com formatação profissional.
- **Importação de Dados**: Importar transações via arquivo CSV com validações robustas.

## Tecnologias

- **Framework**: .NET 10
- **Framework Web**: ASP.NET Core
- **ORM**: Entity Framework Core
- **Banco de Dados**: PostgreSQL
- **Autenticação**: Supabase
- **Geração de PDF**: QuestPDF
- **Processamento CSV**: CsvHelper
- **Tarefas em Segundo Plano**: Quartz.NET
- **Cache**: Cache em memória com suporte a cache distribuído

## Pré-requisitos

- SDK do .NET 10
- Banco de dados PostgreSQL
- Conta Supabase para autenticação e armazenamento

## Instalação

1. Clone o repositório:
   ```
   git clone https://github.com/Xcode-sketcher/PraOndeFoi.git
   cd PraOndeFoi
   ```

2. Restaure as dependências:
   ```
   dotnet restore
   ```

3. Configure a conexão do banco de dados em `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=praondefoi;Username=seusuario;Password=suasenha"
     }
   }
   ```

4. Configure as configurações do Supabase em `appsettings.json`:
   ```json
   {
     "Supabase": {
       "Url": "sua-url-supabase",
       "Key": "sua-chave-anon-supabase"
     }
   }
   ```

5. Execute as migrações do banco de dados:
   ```
   dotnet ef database update
   ```

6. Compile e execute a aplicação:
   ```
   dotnet build
   dotnet run
   ```

A API estará disponível em `http://localhost:5122`.

## Uso

### Autenticação

Registrar um novo usuário:
```
POST /api/auth/cadastrar
{
  "email": "usuario@exemplo.com",
  "password": "senhasegura"
}
```

Login:
```
POST /api/auth/entrar
{
  "email": "usuario@exemplo.com",
  "password": "senhasegura"
}
```

### Principais Endpoints da API

- `GET /api/financas/transacoes` - Recuperar transações paginadas
- `POST /api/financas/transacoes` - Criar uma nova transação
- `PUT /api/financas/transacoes/{id}` - Atualizar uma transação
- `DELETE /api/financas/transacoes/{id}` - Excluir uma transação
- `GET /api/financas/orcamentos` - Obter status do orçamento
- `POST /api/financas/orcamentos` - Criar um orçamento
- `GET /api/financas/tags` - Listar tags
- `POST /api/financas/tags` - Criar uma tag
- `DELETE /api/financas/tags/{id}` - Excluir uma tag
- `GET /api/financas/insights` - Obter insights financeiros
- `GET /api/auth/perfil` - Obter perfil do usuário

Para documentação detalhada da API, consulte a coleção do Postman incluída no repositório.

## Estrutura do Projeto

```
PraOndeFoi/
├── Controllers/          # Controladores da API
├── Models/               # Modelos de entidades
├── DTOs/                 # Objetos de transferência de dados
├── Services/             # Serviços de lógica de negócio
├── Repository/           # Camada de acesso a dados
├── Migrations/           # Migrações do banco de dados
├── Properties/           # Propriedades do projeto
├── appsettings.json      # Configuração
├── Program.cs            # Ponto de entrada da aplicação
└── PraOndeFoi.csproj     # Arquivo do projeto
```

## Testes

Use o arquivo `PraOndeFoi.http` incluído para testar endpoints no VS Code ou importe a coleção do Postman para testes abrangentes da API.


## Licença

Este projeto está licenciado sob a Licença MIT - consulte o arquivo [LICENSE](LICENSE) para detalhes.
