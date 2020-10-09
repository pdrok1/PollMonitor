# PollMonitor
### An API for creating polls and accepting votes


Methods:

/api/poll/active

_Retorna uma lista de polls ativas_

/api/poll/inactive

_Retorna uma lista de polls inativas_

/api/poll/create?question=What polling question do you want to be shown?&selectableOptionsCount=2&limitDate=2020-10-08T19:55:
RequestBody: ["option 1", "some other option", "yet another option"]

_Valida e cria uma poll, e a retorna. 
É possível passar como parâmetro o texto a ser mostrado na pergunta da poll, quantas opções os votantes podem escolher no máximo e a data de fechamento automático da poll._

/api/poll/{id}
_Retorna a poll com esse id_

/api/vote/{id}?1=true&2=true

_Vota na poll com esse {id}._ No exemplo, votou na opção 1 ("option 1") e na opção 2 ("some other option")

>  Pedro Queiroz
