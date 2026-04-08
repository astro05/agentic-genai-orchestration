# 🤖 Agentic GenAI Orchestration

> Scalable multi-model AI workflows powered by GitHub Models, Ollama, RAG, CRAG, AI Agents, S/LLMs, and MCP.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python](https://img.shields.io/badge/Python-3.10%2B-blue.svg)](https://www.python.org/)
[![Ollama](https://img.shields.io/badge/Ollama-Local%20LLM-black)](https://ollama.com/)
[![GitHub Models](https://img.shields.io/badge/GitHub-Models-181717?logo=github)](https://github.com/marketplace/models)

---

## 📖 Overview

**Agentic GenAI Orchestration** is a modular, extensible framework for building intelligent, multi-agent AI systems. It unifies cloud-hosted and locally-run LLMs under a single orchestration layer, enabling retrieval-augmented generation, corrective reasoning, tool-using agents, and inter-agent communication — all at scale.

Whether you're prototyping a RAG pipeline or deploying a production-grade multi-agent workflow, this repository provides the building blocks to do it efficiently.

---

## ✨ Key Features

| Feature | Description |
|---|---|
| 🌐 **GitHub Models** | Plug-and-play integration with GitHub's hosted model marketplace |
| 🦙 **Ollama** | Run open-source LLMs (Llama, Mistral, Phi, etc.) locally with zero cloud cost |
| 📚 **RAG** | Retrieval-Augmented Generation with vector stores for grounded, factual responses |
| 🔄 **CRAG** | Corrective RAG — self-evaluating retrieval with fallback web search for higher accuracy |
| 🤝 **AI Agents** | Tool-using, goal-oriented agents with memory and multi-step reasoning |
| ⚡ **S/LLMs** | Seamless switching between Small LLMs (efficiency) and Large LLMs (capability) |
| 🔌 **MCP** | Model Context Protocol for structured, scalable inter-model communication |
| 🔀 **Multi-Model Workflows** | Orchestrate heterogeneous models across cloud and local runtimes |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Orchestration Layer                    │
│              (Routing · Planning · Memory)              │
└──────────┬──────────────────────────────┬───────────────┘
           │                              │
   ┌───────▼────────┐            ┌────────▼────────┐
   │  GitHub Models │            │     Ollama      │
   │  (Cloud LLMs)  │            │  (Local S/LLMs) │
   └───────┬────────┘            └────────┬────────┘
           │                              │
           └──────────────┬───────────────┘
                          │
              ┌───────────▼────────────┐
              │      AI Agents         │
              │  Tool Use · Memory ·   │
              │  Multi-step Reasoning  │
              └───────────┬────────────┘
                          │
          ┌───────────────┼──────────────────┐
          │               │                  │
   ┌──────▼──────┐ ┌──────▼──────┐  ┌───────▼──────┐
   │     RAG     │ │    CRAG     │  │     MCP      │
   │  Pipeline   │ │  Corrective │  │   Protocol   │
   │             │ │  Retrieval  │  │              │
   └──────┬──────┘ └──────┬──────┘  └───────┬──────┘
          │               │                  │
   ┌──────▼───────────────▼──────────────────▼──────┐
   │           Vector Store · Knowledge Base         │
   └─────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
agentic-genai-orchestration/
│
├── agents/                    # AI agent definitions and tools
│   ├── base_agent.py
│   ├── tool_agent.py
│   └── multi_agent_runner.py
│
├── rag/                       # RAG & CRAG pipelines
│   ├── retriever.py
│   ├── rag_pipeline.py
│   └── crag_pipeline.py
│
├── models/                    # Model connectors
│   ├── github_models.py       # GitHub Models API integration
│   ├── ollama_client.py       # Ollama local model client
│   └── model_router.py        # S/LLM routing logic
│
├── mcp/                       # Model Context Protocol
│   ├── mcp_server.py
│   └── mcp_client.py
│
├── workflows/                 # End-to-end orchestrated workflows
│   ├── research_workflow.py
│   └── qa_workflow.py
│
├── vector_store/              # Embedding & vector DB integration
│   ├── embeddings.py
│   └── vector_store.py
│
├── configs/                   # Configuration files
│   └── config.yaml
│
├── notebooks/                 # Jupyter notebooks & demos
│
├── tests/                     # Unit & integration tests
│
├── requirements.txt
├── .env.example
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites

- Python 3.10+
- [Ollama](https://ollama.com/download) installed and running locally
- A [GitHub Models](https://github.com/marketplace/models) API token
- (Optional) A vector database — ChromaDB, Qdrant, or FAISS

### 1. Clone the Repository

```bash
git clone https://github.com/astro05/agentic-genai-orchestration.git
cd agentic-genai-orchestration
```

### 2. Install Dependencies

```bash
pip install -r requirements.txt
```

### 3. Configure Environment Variables

```bash
cp .env.example .env
```

Edit `.env` with your credentials:

```env
# GitHub Models
GITHUB_TOKEN=your_github_token_here
GITHUB_MODEL=gpt-4o  # or any model from the GitHub marketplace

# Ollama
OLLAMA_BASE_URL=http://localhost:11434
OLLAMA_MODEL=llama3.2

# Vector Store
VECTOR_STORE_TYPE=chroma   # chroma | qdrant | faiss
VECTOR_STORE_PATH=./data/vector_store

# MCP
MCP_SERVER_HOST=localhost
MCP_SERVER_PORT=8765
```

### 4. Pull a Local Model via Ollama

```bash
ollama pull llama3.2
```

### 5. Run a Workflow

```bash
python workflows/qa_workflow.py --query "Explain CRAG and how it differs from RAG"
```

---

## 🧩 Core Modules

### 🌐 GitHub Models Integration

Access frontier models (GPT-4o, Mistral, Phi-4, etc.) via GitHub's model marketplace with a unified API interface.

```python
from models.github_models import GitHubModelClient

client = GitHubModelClient(model="gpt-4o")
response = client.chat("Summarize the key differences between RAG and CRAG.")
print(response)
```

### 🦙 Ollama (Local LLMs)

Run open-source models on-device — no API costs, no data leaving your machine.

```python
from models.ollama_client import OllamaClient

client = OllamaClient(model="llama3.2")
response = client.chat("What is model context protocol?")
print(response)
```

### 📚 RAG Pipeline

Retrieval-Augmented Generation: index your documents, retrieve relevant chunks, and generate grounded answers.

```python
from rag.rag_pipeline import RAGPipeline

pipeline = RAGPipeline(docs_path="./data/docs")
pipeline.index()
answer = pipeline.query("What are the benefits of agentic AI?")
print(answer)
```

### 🔄 CRAG Pipeline

Corrective RAG adds a self-evaluation step — if retrieved documents are deemed irrelevant, it falls back to web search before generating a response.

```python
from rag.crag_pipeline import CRAGPipeline

pipeline = CRAGPipeline()
answer = pipeline.query("Latest benchmarks for open-source LLMs in 2025")
print(answer)
```

### 🤝 AI Agents

Build autonomous agents that reason, use tools, and maintain memory across multi-step tasks.

```python
from agents.tool_agent import ToolAgent

agent = ToolAgent(tools=["web_search", "calculator", "code_executor"])
result = agent.run("Research the top 3 vector databases and compare their performance.")
print(result)
```

### 🔌 MCP (Model Context Protocol)

Enable structured communication between models, agents, and services for composable AI systems.

```python
from mcp.mcp_client import MCPClient

client = MCPClient(server="localhost:8765")
response = client.send(context={"task": "summarize"}, payload={"text": "..."})
```

### ⚡ S/LLM Routing

Automatically route tasks to small or large LLMs based on complexity, latency, and cost requirements.

```python
from models.model_router import ModelRouter

router = ModelRouter(
    small_model="ollama/phi3",
    large_model="github/gpt-4o",
    strategy="complexity"   # complexity | cost | latency
)
response = router.route("What is 2+2?")   # → uses small model
response = router.route("Write a full literature review on transformer architectures.")  # → uses large model
```

---

## 🔬 Workflow Examples

### Research Agent Workflow

A multi-step agent that searches, retrieves, corrects, and synthesizes information using CRAG + GitHub Models.

```bash
python workflows/research_workflow.py --topic "Agentic AI in production systems"
```

### Multi-Agent Q&A Workflow

Multiple specialized agents collaborate: one retrieves, one reasons, one verifies — orchestrated via MCP.

```bash
python workflows/qa_workflow.py --query "Compare RAG and CRAG for enterprise use cases"
```

---

## 🧪 Running Tests

```bash
pytest tests/ -v
```

---

## 🛣️ Roadmap

- [x] GitHub Models integration
- [x] Ollama local model support
- [x] Basic RAG pipeline
- [x] CRAG with corrective retrieval
- [x] Tool-using AI agents
- [x] MCP server/client implementation
- [x] S/LLM routing
- [ ] LangGraph-based multi-agent orchestration
- [ ] Streaming support across all model backends
- [ ] Agent memory with long-term persistence
- [ ] Web UI dashboard for workflow visualization
- [ ] Docker / Compose deployment setup
- [ ] Benchmarking suite for S/LLM routing strategies

---

## 🤝 Contributing

Contributions are welcome! Please open an issue first to discuss what you'd like to change.

1. Fork the repository
2. Create your feature branch: `git checkout -b feature/my-feature`
3. Commit your changes: `git commit -m 'Add my feature'`
4. Push to the branch: `git push origin feature/my-feature`
5. Open a Pull Request

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🙏 Acknowledgements

- [Ollama](https://ollama.com/) — for making local LLMs accessible
- [GitHub Models](https://github.com/marketplace/models) — for democratizing access to frontier models
- [LangChain](https://github.com/langchain-ai/langchain) — RAG and agent tooling
- [ChromaDB](https://www.trychroma.com/) — vector store
- The open-source AI community 🌍

---

<p align="center">Built with ❤️ for the Agentic AI era</p>
