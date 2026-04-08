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
