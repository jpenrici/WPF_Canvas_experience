#WPF_Canvas_experience

Aplicativo construido em WPF C# para manipulação de Shapes e Imagens.

##Principais Funcionalidades

###Menu File
- Save e Save As: Exporta em arquivo .TXT ou .CSV o histórico de inserções e movimento de objetos dentro do Canvas.
- Export: Exporta a imagem do Canvas.
</br>

###Menu Edit
- Clear: Limpa o Canvas, eliminando todos os objetos adicionados, exceto o background.
- Undo/Redo: Operações de desfazer e refazer ações no Canvas.</br>
Observação: Na operação DEL não ocorre a deleção do objeto, apenas a ocultação.
- Background: Altera o fundo do Cavas, com Cor sólida ou com adição de Imagem.</br>
Há a opção Clear, neste caso o background é recomposto no padrão (branco).

###Menu Add
- Adiciona Shapes e Imagens.
Rectângulo: Onde é testado o comportamento de Geometry.
Ellipse: Onde é testado o comportamento de Shape.
Imagem: Onde é possível inserir imagem nos formatos JPG, BMP e PNG.

###Menu About
- Simples mensagem.

###Console
Abaixo da área de desenho (Canvas) é exibido um TextBox com as movimentações executadas.


##Referência
[WPF](https://docs.microsoft.com/pt-br/visualstudio/designers/getting-started-with-wpf?view=vs-2019)

##Display
![Display]()