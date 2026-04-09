# PromptHQ

A Clean Architecture, production-ready ASP.NET Core bot that acts as an AI Prompt Engineering specialist.
It receives basic prompts from users via Telegram or Discord and transforms them into expert-level prompts via OpenAI-compatible APIs (like Groq, OpenRouter, or OpenAI proper).

## Architecture

This project strictly adheres to Clean Architecture principles:
- **PromptHQ.Domain**: Core enums and static constants.
- **PromptHQ.Application**: Core interfaces and business logic (`PromptProcessingService`). It depends on a generic chat client abstraction, not Telegram directly.
- **PromptHQ.Communication**: Communication-channel adapters and webhooks. Telegram and Discord live here now; Slack or another medium can be added here later.
- **PromptHQ.Infrastructure**: Shared infrastructure services such as in-memory user state.
- **PromptHQ.AI**: AI provider configuration and implementations.
- **PromptHQ.API**: ASP.NET Core host that wires the application, infrastructure, AI, and communication modules together.

## Prerequisites

- .NET 9 SDK
- Telegram Bot Token (from BotFather on Telegram)
- Discord Application Public Key for Discord interactions
- API key from an AI provider. Groq, OpenRouter, OpenAI, and direct DeepSeek-compatible providers are configured.
- **ngrok** or Visual Studio dev tunnels to expose your local port

## How to Run Locally

1. Open `PromptHQ.API / appsettings.json`.
2. Fill in your Telegram token, Discord public key, and AI settings:
   - Example for Groq: 
     - BaseUrl: `https://api.groq.com/openai/v1/`
     - ModelName: `llama3-8b-8192` (or similar depending on what is free/available)
   - Example for direct DeepSeek:
     - BaseUrl: `https://api.deepseek.com`
     - ModelName: `deepseek-chat` or `deepseek-reasoner`
     - ApiKey: `YOUR_DEEPSEEK_API_KEY`
3. Set your `SecretToken` (Create any random secure string, e.g., `SuperSecretWebhookToken123`).
4. Ensure your local server is running (usually `https://localhost:5001`).
5. Open a terminal and run ngrok:
   ```bash
   ngrok http 5001
   ```
6. Take the ngrok URL (e.g., `https://abcdef.ngrok-free.app`) and set up your Telegram webhook. You can do this by sending a `POST` request or browser `GET`:
   ```text
   https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook?url=https://abcdef.ngrok-free.app/api/telegram/webhook&secret_token=SuperSecretWebhookToken123
   ```
7. For Discord, set the app's Interactions Endpoint URL in the Discord Developer Portal to:
   ```text
   https://abcdef.ngrok-free.app/api/discord/interactions
   ```
8. Register Discord slash commands for `start`, `model`, `improve` with a required string option named `prompt`, and `elite` with a required string option named `prompt`.
9. Start the application. Send `/start` or `/improve write an email` to your bot on Telegram or Discord.

## Deployment Instructions

When deploying to a production server (like Azure App Service, AWS, or DigitalOcean):
1. **Never** put secrets in `appsettings.json` for production. Use Environment Variables:
   - `TelegramSettings__BotToken`
   - `TelegramSettings__SecretToken`
   - `DiscordSettings__PublicKey`
   - `AIProvidersConfig__Providers__DeepSeekChat__ApiKey`
   - `AIProvidersConfig__Providers__DeepSeekReasoner__ApiKey`
   - `AISettings__ApiKey`
2. Update webhooks dynamically: When your app starts online, configure a startup service or manually set the Telegram webhook to your production domain (`https://apibot.yourcompany.com/api/telegram/webhook`) and the Discord Interactions Endpoint URL to `https://apibot.yourcompany.com/api/discord/interactions`.
3. Behind a reverse proxy (like Nginx), make sure ASP.NET Core allows forwarded headers so webhook limits and validations apply properly.
