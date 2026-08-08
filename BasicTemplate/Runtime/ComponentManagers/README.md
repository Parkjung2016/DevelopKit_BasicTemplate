# ComponentManager

한 오브젝트 아래에 나뉘어 있는 기능 컴포넌트를 순서대로 초기화하고 갱신할 때 사용합니다.
전역 서비스 저장소인 `GlobalRegistry<T>`와 달리, 특정 Owner에 속한 컴포넌트의 생명주기를 관리합니다.

## 기본 사용법

```csharp
public sealed class Character : BaseComponentOwner<Character>
{
    private void Awake()
    {
        InitComponent(this);
    }
}

[ComponentOrder(-100)]
public sealed class CharacterMovement : MonoBehaviour,
    IObjectComponent<Character>, IAfterInitable, IUpdatable
{
    private Character owner;

    public void Initialize(Character character)
    {
        owner = character;
    }

    public void AfterInitialize()
    {
        // 다른 관리 컴포넌트도 초기화된 뒤 필요한 연결을 처리합니다.
    }

    public void OnUpdate()
    {
        // Character의 Update에서 자동으로 호출됩니다.
    }
}
```

`ComponentOrder` 값이 낮은 컴포넌트부터 `Initialize`, `AfterInitialize`, `OnUpdate`가 호출됩니다.
같은 값이면 Hierarchy에서 발견된 순서를 유지합니다. 비활성 자식도 수집합니다.

`BaseComponentOwner.Update`를 재정의할 때는 관리 컴포넌트가 계속 갱신되도록
`base.Update()`를 호출해야 합니다.

## 직접 사용

`BaseComponentOwner<T>`를 상속할 수 없다면 `ComponentManager`를 필드로 두고 다음 순서로 호출합니다.

1. `AddComponentToDictionary(owner)`
2. `ComponentInitialize(owner)`
3. `AfterInitialize()`
4. Owner의 `Update`에서 `OnUpdate()`

Hierarchy 구성이 바뀌어 다시 수집해야 할 때는 1~3단계를 다시 실행하면 됩니다.
기존 목록은 자동으로 비워지므로 콜백이 중복 등록되지 않습니다.
