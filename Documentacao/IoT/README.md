# Relatório IoT

O arquivo `RelatorioIoT.tex` documenta o circuito e inclui diretamente os artefatos executáveis em `Arduino/VinheriaSensores`. Dessa forma, mudanças no circuito ou no firmware aparecem na próxima geração do PDF.

Para gerar o documento com uma instalação TeX completa:

```bash
cd Documentacao/IoT
pdflatex -interaction=nonstopmode -halt-on-error RelatorioIoT.tex
pdflatex -interaction=nonstopmode -halt-on-error RelatorioIoT.tex
```

A segunda execução atualiza referências e numeração.
