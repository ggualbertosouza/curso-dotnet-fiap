# Testando CORS pelo Postman

## Importante: Postman não é navegador

O Postman **não aplica a política de mesma origem** (Same-Origin Policy) — ele não é um browser, então nunca vai "bloquear" a resposta como o Chrome/Firefox fazem. Ele te mostra a resposta completa independente dos headers de CORS.

Isso significa que pelo Postman **não dá pra reproduzir o erro que o aluno vê no navegador**. O que dá pra fazer é **inspecionar os headers que a API devolve** pra cada `Origin` que você mandar, e concluir se o navegador aceitaria ou bloquearia aquela resposta. É uma forma de debugar/validar a configuração de CORS no back-end, não de simular o bloqueio.

> Pra mostrar o bloqueio de verdade acontecendo, use a demo em `cors-demo/index.html` (via navegador), documentada no README ao lado.

## 1. Suba a API

```bash
cd Curso.Api
dotnet run --launch-profile http
```

Ela sobe em `http://localhost:5291`.

## 2. Monte a requisição no Postman

- **Método**: `GET`
- **URL**: `http://localhost:5291/Student`
- Vá na aba **Headers** e adicione manualmente um header `Origin`, simulando de onde o navegador "estaria" chamando:

| Header | Valor |
|---|---|
| `Origin` | `http://localhost:8080` |

Dê **Send**.

## 3. Veja os headers de resposta

Na aba **Headers** da resposta (ou em **View → Show Postman Console**, que mostra a requisição/resposta crua), procure por:

```
Access-Control-Allow-Origin: http://localhost:8080
```

Se esse header aparece com o mesmo valor que você mandou em `Origin`, significa que **um navegador real aceitaria** essa resposta — a política liberou aquela origem.

## 4. Troque para uma origem não permitida

Edite o header:

| Header | Valor |
|---|---|
| `Origin` | `http://127.0.0.1:8080` |

Dê **Send** de novo.

O Postman vai mostrar o **corpo da resposta normalmente** (200 OK, com os dados) — porque, de novo, ele não bloqueia nada. A diferença está nos headers: repare que agora **o header `Access-Control-Allow-Origin` não vem** na resposta (ou vem com um valor diferente do `Origin` enviado).

É exatamente a ausência desse header que faz o **navegador** (não o Postman) descartar a resposta e mostrar o erro de CORS no console — o mesmo erro que aparece na demo em `index.html` ao abrir por `http://127.0.0.1:8080`.

## Resumindo pra explicar em aula

| O que você manda | O que a API responde | O que o Postman faz | O que o navegador faria |
|---|---|---|---|
| `Origin: http://localhost:8080` | inclui `Access-Control-Allow-Origin: http://localhost:8080` | mostra a resposta | aceita a resposta |
| `Origin: http://127.0.0.1:8080` | **não** inclui o header (ou host diferente) | mostra a resposta igual | **bloqueia** a resposta, mesmo ela tendo chegado |

Ou seja: a API sempre responde do mesmo jeito — quem decide bloquear ou não é o navegador, com base nesses headers. O Postman é uma ferramenta boa pra **conferir se o header está correto**, mas não pra demonstrar o bloqueio em si.

## Bônus: testando o preflight (OPTIONS)

Requisições "simples" (como um `GET` sem headers customizados) não disparam preflight. Mas se você quiser ver como seria um preflight de um `POST` com `Content-Type: application/json`, monte manualmente:

- **Método**: `OPTIONS`
- **URL**: `http://localhost:5291/Student`
- **Headers**:
  | Header | Valor |
  |---|---|
  | `Origin` | `http://localhost:8080` |
  | `Access-Control-Request-Method` | `POST` |
  | `Access-Control-Request-Headers` | `content-type` |

Na resposta, verifique os headers `Access-Control-Allow-Methods` e `Access-Control-Allow-Headers` — é isso que o navegador consulta *antes* de mandar o `POST` de verdade, pra decidir se pode prosseguir.
