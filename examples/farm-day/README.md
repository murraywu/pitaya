# Farm Day - Exemplo de Coleta Manual de Recursos

Este diretório contém exemplos de código que demonstram como implementar a mecânica de coleta manual de pedra e madeira no Farm Day.

## Problema Resolvido

Quando recursos (pedra/madeira) nascem dentro da área de colisão do farmer, eles eram coletados automaticamente. Com esta solução, o farmer precisa se mover para que o colisor seja ativado corretamente.

## Arquivos

| Arquivo | Descrição |
|---------|-----------|
| `CollectableResource.cs` | Classe base com proteção de spawn |
| `Stone.cs` | Implementação do recurso pedra |
| `Wood.cs` | Implementação do recurso madeira |
| `FarmerCollector.cs` | Componente de coleta do farmer |
| `ResourceSpawner.cs` | Spawner de recursos com integração |

## Como Funciona

1. **Spawn Protection**: Quando um recurso nasce, ele verifica quais coletores já estão dentro da sua área de colisão e os registra.

2. **Entrada Ignorada**: Se um coletor registrado tentar coletar o recurso sem ter saído da área primeiro, a coleta é bloqueada.

3. **Saída Limpa**: Quando o farmer sai da área de colisão, ele é removido da lista de "estava no spawn".

4. **Coleta Liberada**: Na próxima vez que o farmer entrar na área, a coleta acontece normalmente.

## Uso

### Configuração do Prefab de Recurso

1. Adicione o componente `Stone` ou `Wood` ao prefab do recurso
2. Configure um `Collider2D` como trigger
3. Defina as configurações no Inspector

### Configuração do Farmer

1. Adicione o componente `FarmerCollector` ao farmer
2. Configure a tag do farmer como "Player"
3. Certifique-se de que o farmer tem um `Collider2D`

### Configuração do Spawner

1. Adicione o componente `ResourceSpawner` a um GameObject vazio
2. Arraste os prefabs de recursos para o array `resourcePrefabs`
3. Configure o intervalo e raio de spawn

## Testes

Para testar a implementação:

1. Coloque o farmer perto de um ponto de spawn
2. Aguarde um recurso nascer
3. Verifique que o recurso NÃO é coletado automaticamente
4. Mova o farmer para fora e de volta para a área
5. Verifique que o recurso é coletado agora
