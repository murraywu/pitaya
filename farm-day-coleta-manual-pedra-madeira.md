# Farm Day - Coleta Manual de Pedra e Madeira

## Problema

Atualmente, quando recursos como pedra e madeira nascem (spawn) no jogo, eles são coletados automaticamente se o farmer já estiver dentro da área do colisor. Isso acontece porque a detecção de colisão é acionada imediatamente quando o recurso é instanciado.

**Comportamento atual (indesejado):**
- Recurso nasce → Colisor é ativado → Recurso é coletado automaticamente se farmer estiver próximo

**Comportamento desejado:**
- Recurso nasce → Farmer precisa se mover → Colisor é ativado → Recurso é coletado

## Solução Proposta

A solução envolve adicionar um estado de "recém-nascido" (freshly spawned) aos recursos coletáveis. Durante este estado, o recurso ignora colisões com o coletor do farmer. O recurso só se torna coletável quando:

1. O farmer se move (detectado por mudança de posição), OU
2. O farmer sai e entra novamente na área de colisão

### Implementação

#### Opção 1: Flag de Spawn com Detecção de Movimento (Recomendada)

Adicionar uma flag `isNewlySpawned` ao recurso que é desativada quando o farmer se move:

```csharp
// CollectableResource.cs
public class CollectableResource : MonoBehaviour
{
    [SerializeField] private bool isNewlySpawned = true;
    private HashSet<Collider2D> collidersInsideOnSpawn = new HashSet<Collider2D>();
    
    private void Start()
    {
        isNewlySpawned = true;
        // Registrar colisores que já estão dentro no momento do spawn
        StartCoroutine(RegisterInitialColliders());
    }
    
    private IEnumerator RegisterInitialColliders()
    {
        // Aguardar um frame para a física processar
        yield return new WaitForFixedUpdate();
        
        Collider2D[] overlapping = Physics2D.OverlapCircleAll(
            transform.position, 
            GetComponent<CircleCollider2D>().radius
        );
        
        foreach (var col in overlapping)
        {
            if (col.CompareTag("Player") || col.CompareTag("Farmer"))
            {
                collidersInsideOnSpawn.Add(col);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Farmer"))
            return;
            
        // Se o coletor não estava dentro no spawn, pode coletar
        if (!collidersInsideOnSpawn.Contains(other))
        {
            Collect(other.GetComponent<FarmerCollector>());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Quando o coletor sai, remover da lista de "estava no spawn"
        collidersInsideOnSpawn.Remove(other);
    }
    
    public void Collect(FarmerCollector collector)
    {
        if (collector == null) return;
        
        // Lógica de coleta
        collector.AddResource(this);
        Destroy(gameObject);
    }
}
```

#### Opção 2: Verificar Movimento do Farmer

Alternativamente, verificar se o farmer se moveu desde o spawn do recurso:

```csharp
// CollectableResource.cs
public class CollectableResource : MonoBehaviour
{
    private Vector3 farmerPositionOnSpawn;
    private bool hasTrackedFarmerPosition = false;
    private const float MOVEMENT_THRESHOLD = 0.1f;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Farmer"))
            return;
            
        if (!hasTrackedFarmerPosition)
        {
            // Primeira vez que detectamos o farmer - salvar posição
            farmerPositionOnSpawn = other.transform.position;
            hasTrackedFarmerPosition = true;
            return;
        }
        
        // Verificar se o farmer se moveu desde o spawn
        float distanceMoved = Vector3.Distance(
            farmerPositionOnSpawn, 
            other.transform.position
        );
        
        if (distanceMoved > MOVEMENT_THRESHOLD)
        {
            Collect(other.GetComponent<FarmerCollector>());
        }
    }
    
    public void Collect(FarmerCollector collector)
    {
        if (collector == null) return;
        
        collector.AddResource(this);
        Destroy(gameObject);
    }
}
```

#### Opção 3: Delay Simples no Spawn

Uma solução mais simples, porém menos precisa:

```csharp
// CollectableResource.cs
public class CollectableResource : MonoBehaviour
{
    [SerializeField] private float spawnProtectionTime = 0.5f;
    private bool canBeCollected = false;
    
    private IEnumerator Start()
    {
        canBeCollected = false;
        yield return new WaitForSeconds(spawnProtectionTime);
        canBeCollected = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected) return;
        
        if (other.CompareTag("Player") || other.CompareTag("Farmer"))
        {
            Collect(other.GetComponent<FarmerCollector>());
        }
    }
    
    public void Collect(FarmerCollector collector)
    {
        if (collector == null) return;
        
        collector.AddResource(this);
        Destroy(gameObject);
    }
}
```

## Recomendação

**Opção 1** é a mais recomendada porque:
- Não depende de tempo (sem delays arbitrários)
- Garante que o farmer precisa realmente sair e entrar na área de colisão
- Funciona corretamente mesmo se o farmer estiver parado exatamente sobre o ponto de spawn

## Arquivos a Modificar

Os arquivos que provavelmente precisam ser modificados no projeto Farm Day:

1. `CollectableResource.cs` ou similar - Script dos recursos coletáveis
2. `Stone.cs` / `Wood.cs` - Se houver scripts específicos para pedra/madeira
3. `ResourceSpawner.cs` - Se a lógica de spawn precisar passar informações para o recurso

## Testes

Cenários de teste para validar a implementação:

1. **Farmer parado, recurso nasce dentro da área de coleta**
   - Esperado: Recurso NÃO é coletado automaticamente
   
2. **Farmer se move após recurso nascer dentro da área**
   - Esperado: Recurso é coletado quando farmer se move
   
3. **Farmer sai e volta para a área do recurso**
   - Esperado: Recurso é coletado na re-entrada
   
4. **Recurso nasce fora da área, farmer entra**
   - Esperado: Recurso é coletado normalmente (comportamento original)

5. **Múltiplos recursos nascem ao mesmo tempo**
   - Esperado: Cada recurso tem seu próprio estado independente
