# Privacy Notice

**Last Updated:** November 21, 2025

This document provides transparency about what data the Teknologi Umum Telegram Bot ("the bot") sends to third-party services when you interact with it. This is **not a privacy policy**, but rather an audit report of our third-party integrations.

## What This Bot Does

This bot is an open-source Telegram bot that provides various services including AI chat, code execution, image generation, weather information, and more. When you use certain commands, your input may be sent to third-party services to process your request.

## Third-Party Services That Receive Your Data

### 1. OpenAI (api.openai.com)

**Commands:** `/ask` and AI conversation features  
**Data Sent:**
- Your text prompts and questions
- Conversation history (up to 10 previous messages in the thread)
- Image generation prompts (when using DALL-E features)

**Purpose:** To generate AI responses to your questions and create images from text descriptions.

**Privacy Policy:** https://openai.com/policies/privacy-policy

**Data Retention:** OpenAI may retain your data according to their privacy policy. As of the last update, OpenAI states they retain API data for 30 days for abuse monitoring purposes, then delete it unless legally required to keep it longer.

---

### 2. Google Gemini (generativelanguage.googleapis.com)

**Commands:** AI conversation features  
**Data Sent:**
- Your text prompts and questions
- Conversation context and message history

**Purpose:** To generate AI responses using Google's Gemini model.

**Privacy Policy:** https://policies.google.com/privacy

**Data Retention:** Google may process and store your data according to their privacy policy.

---

### 3. Stability AI (api.stability.ai)

**Commands:** `/art` and image generation features  
**Data Sent:**
- Text prompts for image generation
- Images you upload (for image-to-image transformation)

**Purpose:** To generate or modify images based on your text descriptions.

**Privacy Policy:** https://stability.ai/privacy-policy

**Data Retention:** Stability AI processes your data according to their privacy policy.

---

### 4. Craiyon (backend.craiyon.com)

**Commands:** Image generation features  
**Data Sent:**
- Text prompts for image generation

**Purpose:** To generate AI-created images from your text descriptions.

**Privacy Policy:** https://www.craiyon.com/privacy-policy

**Data Retention:** Craiyon processes your data according to their privacy policy.

---

### 5. Google Maps API (maps.googleapis.com)

**Commands:** `/map`  
**Data Sent:**
- Location names and search queries
- Place IDs

**Purpose:** To provide location information and place details.

**Privacy Policy:** https://policies.google.com/privacy

**Data Retention:** Google may process and store your location searches according to their privacy policy.

---

### 6. Piston Code Execution Engine (piston-meta.tecnm.dev)

**Commands:** `/python`, `/java`, `/c`, `/cpp`, `/js`, `/ts`, `/go`, `/rust`, and other programming language commands  
**Data Sent:**
- Your source code
- Programming language identifier

**Purpose:** To execute your code in a sandboxed environment and return the output.

**Service Type:** Community-hosted service

**Privacy Policy:** https://github.com/engineer-man/piston (open source project)

**Data Retention:** Code is executed in ephemeral containers and not persisted after execution completes.

**Note:** The bot currently uses Piston for all code execution commands. Pesto is configured as an alternative service but is not actively used at this time.

---

### 7. OMDb API (omdbapi.com)

**Commands:** `/movie`  
**Data Sent:**
- Movie or TV show titles you search for

**Purpose:** To retrieve information about movies and TV shows.

**Privacy Policy:** http://www.omdbapi.com/legal.htm

**Data Retention:** OMDb may process search queries according to their terms.

---

### 8. wttr.in Weather Service (wttr.in)

**Commands:** `/weather`  
**Data Sent:**
- Location names or coordinates you provide

**Purpose:** To retrieve weather information for your specified location.

**Service Type:** Open-source weather service

**Privacy Policy:** https://github.com/chubin/wttr.in (open source project)

**Data Retention:** wttr.in is an open-source service; check their repository for data handling practices.

---

### 9. Google Sheets API

**Commands:** Internal data sources  
**Data Sent:**
- Spreadsheet IDs for read-only access

**Purpose:** To retrieve data from configured Google Sheets for bot features.

**Privacy Policy:** https://policies.google.com/privacy

**Data Retention:** Google processes API requests according to their privacy policy.

---

## Services That DO NOT Receive User-Generated Content

The following services are used by the bot but do **not** receive any personal data or user-generated content:

- **BMKG** (data.bmkg.go.id) - Indonesian earthquake data (public data only)
- **NoAsAService** (naas.isalman.dev) - Random response generator (no input required)
- **Primbon websites** - Indonesian traditional fortune-telling (receives search terms only)
- **This X Does Not Exist** services - AI-generated images (no user input sent)
- **KPU Sirekap** - Indonesian election data (public data only)
- **Status Page** services - Service status monitoring (no user input)
- **Various public website scrapers** - Programmer humor, tech benchmarks, etc.
- **Pesto** - Alternative code execution service (configured but not currently in use)

---

## What Data We Store

The bot itself stores:
- **Telegram Message Cache:** Recent messages for conversation threading (stored in memory, not persisted to disk)
- **Rate Limiting Data:** Temporary counters to prevent spam (stored in memory)
- **Metrics:** Anonymous usage statistics (command counts, execution times)

We do **not** store:
- Your Telegram user ID in any persistent database
- Your message history beyond in-memory caching
- Any personal information

---

## Your Control Over Your Data

Since this bot sends your data to third-party services:

1. **Before using sensitive commands:** Consider what data you're sharing. Don't share passwords, API keys, or personal information in prompts.

2. **Code execution:** Be aware that code you send to `/python`, `/java`, etc. is executed on third-party servers.

3. **AI conversations:** Your conversation history is sent to AI providers. Don't share private or sensitive information.

4. **Delete your data:** Contact the respective third-party services directly to request data deletion according to their privacy policies.

5. **Stop using the bot:** You can stop using the bot at any time by simply not sending it commands.

---

## Data Security

- All communications between the bot and third-party services use HTTPS encryption.
- The bot does not store API keys or credentials in user-accessible locations.
- Rate limiting is in place to prevent abuse.

---

## Changes to This Notice

This notice may be updated when:
- New third-party services are integrated
- Existing services change their privacy policies
- Our data handling practices change

Check the "Last Updated" date at the top of this document.

---

## Open Source

This bot is open source. You can review the code at: https://github.com/teknologi-umum/botnet

To see exactly what data is sent to each service, inspect the source code in the `BotNet.Services` directory.

---

## Contact

For questions about this privacy notice or the bot's data handling:
- GitHub Issues: https://github.com/teknologi-umum/botnet/issues
- Community: Teknologi Umum Telegram group

---

## Important Disclaimer

We are **not responsible** for how third-party services handle your data. Each service has its own privacy policy and data retention practices. By using commands that send data to third parties, you agree to their respective terms of service and privacy policies.

**Use at your own risk.** Don't send sensitive, private, or confidential information to this bot.
