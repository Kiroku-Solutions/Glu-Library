<div align="center">
  <img src="https://via.placeholder.com/150/512BD4/FFFFFF?text=Glu+Lib" alt="Glu Library Logo" width="150" height="150">
  
  # Glu Library
  
  ### The Enterprise .NET Client for Soniox AI
  
  **Robust, Resilient & Secure WebSocket wrapper for Real-Time Transcription**
  
  [![.NET](https://img.shields.io/badge/.NET-8.0%2F9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
  [![NuGet](https://img.shields.io/badge/NuGet-v1.0.0-blue?logo=nuget&logoColor=white)](https://www.nuget.org/)
  [![Architecture](https://img.shields.io/badge/Architecture-Direct_Stream-brightgreen?logo=architecture&logoColor=white)]()
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
  [![Resilience](https://img.shields.io/badge/Resilience-Auto--Reconnect-orange)]()
  
</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Architecture](#%EF%B8%8F-architecture)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Configuration](#-configuration)
- [Usage](#-usage)
- [Advanced Features](#-advanced-features)

---

## 🎯 Overview

**Glu Library** is the official .NET middleware designed to integrate **Soniox AI** speech recognition into Blazor, ASP.NET Core, or Console applications. 

Unlike raw WebSocket implementations, Glu Library provides a **production-ready layer** that handles authentication, connection stability, state management, and strict speaker diarization, allowing developers to focus on building UI rather than debugging socket frames.

---

## ✨ Key Features

<table>
  <tr>
    <td align="center" width="33%">
      <h3>🛡️ Scoped Security</h3>
      <p>Designed with <code>AddScoped</code> lifecycle to ensure data isolation between different users in Blazor Server environments.</p>
    </td>
    <td align="center" width="33%">
      <h3>🔄 Auto-Resilience</h3>
      <p>Built-in <strong>Retry Policies</strong>. If the network blips, the library automatically reconnects and restores the session context.</p>
    </td>
    <td align="center" width="33%">
      <h3>⚡ Soniox V3 Native</h3>
      <p>Fully compatible with Soniox V3 API, optimized for ultra-low latency and token-based cost savings.</p>
    </td>
  </tr>
  <tr>
    <td align="center">
      <h3>👥 Dynamic Diarization</h3>
      <p>Runtime configuration allows you to define who is the "Agent" and "Customer" dynamically.</p>
    </td>
    <td align="center">
      <h3>📝 Professional Logging</h3>
      <p>Integrated with <code>ILogger</code> to provide structured telemetry for Azure/AWS monitoring.</p>
    </td>
    <td align="center">
      <h3>🎛️ Session Config</h3>
      <p>Inject API Keys, Context, or Language hints at runtime via UI dropdowns.</p>
    </td>
  </tr>
</table>

---

## 🏗️ Architecture

Glu Library acts as a **smart proxy** inside your .NET application, managing the complex WebSocket handshake with Soniox.

```mermaid
graph LR
    User[👤 User / Browser] -->|Microphone Audio| App[💻 Your Blazor App]
    subgraph "Glu Library (Scoped)"
        Client[🔌 WebSocket Client]
        State[💾 State Manager]
        Logic[🧠 Resilience Logic]
    end
    App --> Client
    Client <-->|Managed WebSocket| Soniox[☁️ Soniox AI Cloud]
    Client -->|Events| State
    State -->|UI Updates| App

    style User fill:#e1f5ff
    style App fill:#512BD4,color:#fff
    style Soniox fill:#ff6b6b,color:#fff
    style Client fill:#ffd700
