### 1.单点登录（Single sign-on, SSO）

```mermaid
flowchart LR
User--->|UserName&Password|AuthorizationServer[Authorization Server]
AuthorizationServer-..->|Token|User
User--->|"/api/orders(token sent via a header)"|APIServer1[API Server1]
APIServer1-..->|Orders|User
User--->|"/api/goods(token sent via a header)"|APIServer2[API Server2]
APIServer2-..->|Goods|User
```

用户登录转发到鉴权中心，验证通过后颁发一个凭证（Token），用户使用这个凭证（Token）去请求其他服务

> Token信任机制：
>
> 1. 需要保证Token来自于鉴权中心
> 2. 需要保证Token没有被篡改

### 2.JSON Web Token, JWT

JWT可用于解决单点登录的Token信任机制，一般使用非对称可逆加密算法（对称可逆加密一般用在公司内部，需要共享私钥）

> 1. 鉴权中心预先生成加密钥（私钥）和解密钥（公钥），把传递数据转成json并用私钥加密
> 2. 其他服务使用鉴权中心公开的公钥对密文进行解密，即可证明Token来自于鉴权中心
> 3. 密文中包含了传递数据MD5摘要的加密，其他服务解密后，再比对传递数据MD5，即可判断Token是否被篡改

> 注意：因为公钥是公开的，即代表JWT令牌内容是公开可见的，不要传递敏感信息

JWT令牌结构，使用两个`.`分隔符将令牌分为三个部分：

```jwt
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

第一部分：头部`HEADER`

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

* alg：加密算法
* typ：声明类型

第二部分：载荷`PAYLOAD`，存放传递的数据，一般是用户信息

```json
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022
}
```

第三部分：签名`VERIFY SIGNATURE`，对第一、二部分进行MD5摘要（32位长度）然后使用密钥加密（其他服务将第三部分解密后，比对第一、二部分的MD5摘要，即可判断Token是否被篡改）

> JWT局限性：
>
> 1. Token泄露问题
> 2. 用户信息变动时不能立即过期问题
> 3. 滑动过期问题

### 3.AccessToken和RefreshToken

使用AccessToken和RefreshToken解决JWT的2、3点局限性

```mermaid
flowchart TB
User-->|UserName&Password|AuthorizationServer[Authorization Server]
AuthorizationServer-.->|AccessToken&RefreshToken|User
User-->|AccessToken&RefreshToken|APIServer[API Server]
APIServer-->AccessTokenExpried{"AccessToken
是否过期?"}
AccessTokenExpried-->|否|Visit(正常访问)
AccessTokenExpried-->|RefreshToken|AuthorizationServer1[Authorization Server]
AuthorizationServer1-->RefreshTokenExpried{"RefreshToken
是否失效?"}
RefreshTokenExpried-->|否|Issued(颁发新的AsscessToken)
Issued-->Visit1(正常访问)
RefreshTokenExpried-->|是|Login(要求重新登录)
```

> 注意：
>
> 1. AccessToken时效短（比如1小时），RefreshToken时效长（比如7天）
> 2. 使用双Token进行滑动过期，用户无感更新AsseccToken，解决上述第3点局限性
> 3. 当用户信息变动时使RefreshToken失效解决上述第2点局限性（不能立即生效，也是需要等待AccecssToken过期后才生效）

> 前端部分：
>
> 1. 登录时，前端获取的AccessToken和RefreshToken保存在LocalStorage中，后续请求将LocalStorage中的AccessToken带到请求头去，Authorization="Bearer xxxxx"
> 2. 若请求时服务端响应AccessToken过期，则请求RefreshToken接口重新获取新的AccessToken，保存到LocalStorage，最后重新发起原来的请求
