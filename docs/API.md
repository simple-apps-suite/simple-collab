# API Reference

> [!NOTE]
> Examples in this document use the following Ed25519 key:\
> **Private key:** `"uExw3pnNoVhz7pk7efAA6lEllKbQTEeJS4qnzxv-uBk"`\
> **Public key:** `"5uUg7dmfzRLUJmfq2xt8GOTHkjuD6iVttcL0wrGpgOc"`\
> **Hash:** `"V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs"`

## Create Identity

Create a new anonymous identity.

### Endpoint

```
POST /api/v1/identity
```

### Request

Field      | Type   | Description
-----------|--------|-----------------------------------------------------------
public_key | string | Public key in base64url format.
pow        | string | PoW 26 on the public key.

<details>
<summary>Example Request</summary>

```json
POST /api/v1/identity HTTP/1.1
Content-Type: application/json

{
  "public_key": "5uUg7dmfzRLUJmfq2xt8GOTHkjuD6iVttcL0wrGpgOc",
  "pow": "TODO"
}
```
</details>

### Response

#### Success

Identity created successfully.

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
hash  | string | `base64url(sha256(public_key))`

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "hash": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs"
}
```
</details>

#### Error

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
error | string | Error code.

**Possible Error Codes:**
Status | Error Code         | Description
-------|--------------------|---------------------------------------------------
400    | malformed_request  | Request body is not valid JSON.
400    | public_key_missing | Public key field missing.
400    | pow_missing        | Proof-of-work field missing.
400    | public_key_invalid | Public key not valid.
400    | pow_invalid        | Proof-of-work not valid.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "pow_invalid"
}
```
</details>


## Create User

Register a new user with a username, using a previously created identity.

### Endpoint

```
POST /api/v1/user
```

### Request

Field     | Type    | Description
----------|---------|-----------------------------------------------------------
timestamp | integer | Current UNIX timestamp in seconds.
identity  | string  | Identity hash in base64url format.
username  | string  | Desired username.
signature | string  | `sign("REGISTER_USER " + base64url(sha256(username)) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"REGISTER_USER " + base64url(sha256("example_user")) + " " + "1608726896"`,
> which is `"REGISTER_USER j3BwXiW6oAwtuKkl1I53mum4elV3uQ1TOcP-8BEeH0A 1608726896"`.

```json
POST /api/v1/user HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "username": "example_user",
  "signature": "wF_ikM-WXqGy-Mt1ArW9hJhtf1L-ye9kec6yV9VwHqllEO4ru2UAeMe4KRjTQ4pCfqRl8VJ74noFjH2Fr6FaCw"
}
```
</details>

### Response

#### Success

User created successfully, or the user already existed and the identity was
already associated with them.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{}
```
</details>

#### Error

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
error | string | Error code.

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
400    | malformed_request | Request body is not valid JSON.
400    | identity_missing  | Identity hash field missing.
400    | username_missing  | Username field missing.
400    | signature_missing | Signature field missing.
400    | username_invalid  | Username does not meet requirements.
400    | signature_invalid | Signature does not match.
404    | unknown_identity  | Identity hash not found.
409    | username_taken    | Username already in use.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 409 Conflict
Content-Type: application/json

{
  "error": "username_taken"
}
```
</details>

## Associate Identity to User

Associate a previously created identity to an existing user.

### Endpoint

```
POST /api/v1/user/identity
```

### Request

Field            | Type    | Description
-----------------|---------|----------------------------------------------------
timestamp        | integer | Current UNIX timestamp in seconds.
current_identity | string  | Hash in base64url format of an identity already associated with the user.
new_identity     | string  | Hash in base64url format of the new identity to associate.
username         | string  | User to associate with.
signature        | string  | `sign("ADD_IDENTITY " + base64url(sha256(base64url(sha256(username)) + new_identity)) + " " + timestamp, private_key(current_identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"ADD_IDENTITY " + base64url(sha256(base64url(sha256("example_user")) + "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8")) + " " + "1608726896"`,
> which is `"ADD_IDENTITY " + base64url(sha256("j3BwXiW6oAwtuKkl1I53mum4elV3uQ1TOcP-8BEeH0Ay4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8")) + " 1608726896"`,
> which is `"ADD_IDENTITY a4rotNE6ptJAWVIfGOfVsjAggvuuIbUBAGSirPYZo3Y 1608726896"`.

```json
POST /api/v1/user/identity HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "current_identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "new_identity": "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8",
  "username": "example_user",
  "signature": "B3XsoxCmrRzvAdhQRVpfm0IfOXHlI2yQ6jSZuu2NTfn72vTIWJexNEudif4c4vZoLmFHW0GehIQZUfpBaB7XCg"
}
```
</details>

### Response

#### Success

Associated successfully, or the identity was already associated with the user.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{}
```
</details>

#### Error

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
error | string | Error code.

**Possible Error Codes:**
Status | Error Code               | Description
-------|--------------------------|---------------------------------------------
400    | malformed_request        | Request body is not valid JSON.
400    | current_identity_missing | Current identity field missing.
400    | new_identity_missing     | New identity field missing.
400    | username_missing         | Username field missing.
400    | signature_missing        | Signature field missing.
400    | signature_invalid        | Signature does not match.
400    | invalid_current_identity | Current identity not associated with the user.
404    | unknown_current_identity | Current identity hash not found.
404    | unknown_new_identity     | New identity hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "signature_invalid"
}
```
</details>

## Disassociate Identity from User

Remove an identity association from a user.

Once the last identity association with a user is removed, the user is
automatically also removed and their username may be used by other identities.

### Endpoint

```
DELETE /api/v1/user/identity
```

### Request

Field     | Type    | Description
----------|---------|-----------------------------------------------------------
timestamp | integer | Current UNIX timestamp in seconds.
identity  | string  | Hash in base64url format of the identity to disassociate.
username  | string  | User to disassociate from.
signature | string  | `sign("REMOVE_IDENTITY " + base64url(sha256(base64url(sha256(username)) + identity)) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"REMOVE_IDENTITY " + base64url(sha256(base64url(sha256("example_user")) + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs")) + " " + "1608726896"`,
> which is `"REMOVE_IDENTITY " + base64url(sha256("j3BwXiW6oAwtuKkl1I53mum4elV3uQ1TOcP-8BEeH0AV7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs")) + " 1608726896"`,
> which is `"REMOVE_IDENTITY b46N84wP43bqgM0erbqrKbZfxbYtupmQ9COZve07Rj0 1608726896"`.

```json
DELETE /api/v1/user/identity HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "username": "example_user",
  "signature": "WU-AbmhxUHe_-IaY2DkPqWPYdrptPSZppOmXFVTt4PxgxEK3A3sYUeBUPUlATOJINroo435Ddn7E9LR37dngAw"
}
```
</details>

### Response

#### Success

Disassociated successfully, or the identity was not associated with the user.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{}
```
</details>

#### Error

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
error | string | Error code.

**Possible Error Codes:**
Status | Error Code              | Description
-------|-------------------------|----------------------------------------------
400    | malformed_request       | Request body is not valid JSON.
400    | identity_missing        | Identity field missing.
400    | username_missing        | Username field missing.
400    | signature_missing       | Signature field missing.
400    | signature_invalid       | Signature does not match.
400    | identity_not_associated | Identity is not associated with the user.
404    | unknown_identity        | Identity hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "identity_not_associated"
}
```
</details>

## Create Document

Create a new document and rent it.

### Endpoint

```
POST /api/v1/document
```

### Request

Field              | Type     | Description
-------------------|----------|-------------------------------------------------
timestamp          | integer  | Current UNIX timestamp in seconds.
identity           | string   | Identity hash in base64url format.
type               | string   | Document type GUID.
data               | string   | Document data in base64url format.
expiration         | integer  | Rent expiration UNIX timestamp in seconds (optional).
signature          | string   | `sign("RENT " + base64url(sha256(base64url(sha256(type + base64url(sha256(data)))) + identity + expiration)) + " " + timestamp, private_key(identity))`
publish_signature  | string   | `sign("PUBLISH " + base64url(sha256(base64url(sha256(type + base64url(sha256(data)))) + identity + expiration)) + " " + timestamp` (optional, only if you want to make the document publicly indexable).
share              | object[] | Array of targets to share with (optional).
share[].identity   | string   | Identity hash to share with, in base64url format.
share[].expiration | integer  | Share invitation expiration UNIX timestamp in seconds (optional, strongly recommended).
share[].signature  | string   | `sign("RENT " + base64url(sha256(base64url(sha256(type + base64url(sha256(data)))) + share_with + expiration)) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"RENT " + base64url(sha256(base64url(sha256("826eca95-0078-434e-b93a-8af087da1a16" + base64url(sha256("Hello, World!")))) + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs" + "1737635696")) + " " + "1608726896"`,
> which is `"RENT " + base64url(sha256(base64url(sha256("826eca95-0078-434e-b93a-8af087da1a163_1gIbsr1bCvZ2KQgJ7DpTGR3YHH9wpLKGiKNiGCmG8")) + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs1737635696")) + " 1608726896"`,
> which is `"RENT " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0QV7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs1737635696")) + " " + timestamp`,
> which is `"RENT 2lMGlyJHHcnL7vpM-e0LCILK5OU8JKvSQaMzS6O8qt0 1608726896"`.
>
> The signature to make this document publicly indexable is over `"PUBLISH " + base64url(sha256(base64url(sha256("826eca95-0078-434e-b93a-8af087da1a16" + base64url(sha256("Hello, World!")))) + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs" + "1737635696")) + " " + "1608726896"`,
> which is `"PUBLISH " + base64url(sha256(base64url(sha256("826eca95-0078-434e-b93a-8af087da1a163_1gIbsr1bCvZ2KQgJ7DpTGR3YHH9wpLKGiKNiGCmG8")) + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs1737635696")) + " 1608726896"`,
> which is `"PUBLISH " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0QV7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs1737635696")) + " " + timestamp`,
> which is `"PUBLISH 2lMGlyJHHcnL7vpM-e0LCILK5OU8JKvSQaMzS6O8qt0 1608726896"`.
>
> The signature for sharing with another identity is over `"RENT " + base64url(sha256(base64url(sha256("826eca95-0078-434e-b93a-8af087da1a16" + base64url(sha256("Hello, World!")))) + "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8" + "1737635696")) + " " + "1608726896"`,
> which is `"RENT " + base64url(sha256(base64url(sha256("826eca95-0078-434e-b93a-8af087da1a163_1gIbsr1bCvZ2KQgJ7DpTGR3YHH9wpLKGiKNiGCmG8")) + "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc81737635696")) + " 1608726896"`,
> which is `"RENT " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Qy4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc81737635696")) + " 1608726896"`,
> which is `"RENT 6pw3gz-gYuodFB2O6m7ZZdhLv3yeA_BdSIPSOxOLILw 1608726896"`.
>
> When the expiration is not set, use `""` in its place.

```json
POST /api/v1/document HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "type": "826eca95-0078-434e-b93a-8af087da1a16",
  "data": "SGVsbG8sIFdvcmxkIQ",
  "expiration": 1737635696,
  "signature": "xH3fbaO2jGR6b8Oy2jYgz-q_hnrwXqXOSVnzcBAz0DjAKtPr5AW0wKq4L_cOZTn8bzk4ejx23ZEyRjJywgBRCg",
  "publish_signature": "MVQAkgDPQEqtkKayrt6Yycv65Y9qaiMEl_0vN9RmCLoCqG3LCvlGfYoIjC4_xDlDEoL1teQmdJC3FbMwxNv_BQ",
  "share": [
    {
      "identity": "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8",
      "expiration": 1735787045,
      "signature": "cBVZT-KibHOCAnmwx03ti57kEBmc3hH4H8EdD6FD4ziK-HBgy54dR230Z2vuhH3BZql8HNBiS3rroiZ9HNUsAg"
    }
  ]
}
```
</details>

Alternatively, it's possible to upload the document data as a separate chunk
with a `multipart/form-data` request. In this case, omit the `data` field from
the JSON part and instead include a `data` file part. The rest of the JSON
fields must be sent as a part named `metadata`.

<details>
<summary>Example Request</summary>

```json
POST /api/v1/document HTTP/1.1
Content-Type: multipart/form-data; boundary=Boundary

--Boundary
Content-Disposition: form-data; name="metadata"
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "type": "826eca95-0078-434e-b93a-8af087da1a16",
  "expiration": 1737635696,
  "signature": "mYA8D9Hqb6DYEZwEsgZ3_HuAtRBArJqxBLV9tQeebFdmzP6ZDzqAMt6VfUYGf3OdfEdBbvhpiUHlBvcNYhNrAQ",
  "publish_signature": "ta_4kw8fMpisb7tBuHHM6AeBVC5vkSd5a8E9wDUNFN5Whc-RynrDffArbRHFF37b5SHQ3SrtqUDsF0U_ePczAA",
  "share": [
    {
      "identity": "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8",
      "expiration": 1735787045,
      "signature": "jGJSKozBKu00vNRDZtsh6yFXI7gExR2ielD58ak9plCQpSPw-16988k4j9bKkqCFDMLBXvcdGl0aJrb3aK9uAg"
    }
  ]
}
--Boundary
Content-Disposition: form-data; name="data"
Content-Type: application/octet-stream

Hello, World!
--Boundary--
```
</details>

### Response

#### Success

Document created successfully.

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
hash  | string | `base64url(sha256(type + base64url(sha256(data))))`

<details>
<summary>Example Response</summary>

> [!NOTE]
> In this example, the hash is `base64url(sha256("826eca95-0078-434e-b93a-8af087da1a16" + base64url(sha256("Hello, World!"))))`,
> which is `base64url(sha256("826eca95-0078-434e-b93a-8af087da1a163_1gIbsr1bCvZ2KQgJ7DpTGR3YHH9wpLKGiKNiGCmG8"))`.

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "hash": "RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q"
}
```
</details>

#### Error

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
error | string | Error code.

**Possible Error Codes:**
Status | Error Code               | Description
-------|--------------------------|---------------------------------------------
400    | malformed_request        | Request body is not valid JSON.
400    | identity_missing         | Identity field missing.
400    | type_missing             | Type field missing.
400    | data_missing             | Data field missing.
400    | signature_missing        | Signature field missing.
400    | type_invalid             | Type is not a valid GUID.
400    | signature_invalid        | Signature does not match.
400    | expiration_invalid       | Expiration is not a valid UNIX timestamp.
400    | public_invalid           | Public is not a boolean.
400    | share_identity_missing   | One or more share identity fields missing.
400    | share_signature_missing  | One or more share signature do not match.
400    | share_expiration_invalid | Expiration is not a valid UNIX timestamp.
404    | unknown_identity         | Identity hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "signature_invalid"
}
```
</details>

## Share Document

Share an existing document with other identities.

### Endpoint

```
POST /api/v1/document/share
```

### Request

Field              | Type     | Description
-------------------|----------|-------------------------------------------------
timestamp          | integer  | Current UNIX timestamp in seconds.
document           | string   | Document hash in base64url format.
identity           | string   | Sharing identity hash in base64url format.
share              | object[] | Array of targets to share with.
share[].identity   | string   | Identity hash to share with, in base64url format.
share[].expiration | integer  | Share invitation expiration UNIX timestamp in seconds (optional, strongly recommended).
share[].signature  | string   | `sign("RENT " + base64url(sha256(document + share_with + expiration)) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"RENT " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q" + "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8" + "1737635696")) + " " + "1608726896"`,
> which is `"RENT " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Qy4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc81737635696")) + " 1608726896"`,
> which is `"RENT 6pw3gz-gYuodFB2O6m7ZZdhLv3yeA_BdSIPSOxOLILw 1608726896"`.
>
> When the expiration is not set, use `""` in its place.

```json
POST /api/v1/document/share HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "document": "RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q",
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "share": [
    {
      "identity": "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8",
      "expiration": 1735787045,
      "signature": "cBVZT-KibHOCAnmwx03ti57kEBmc3hH4H8EdD6FD4ziK-HBgy54dR230Z2vuhH3BZql8HNBiS3rroiZ9HNUsAg"
    }
  ]
}
```
</details>

### Response

#### Success

Document shared successfully.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{}
```
</details>

#### Error

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
error | string | Error code.

**Possible Error Codes:**
Status | Error Code               | Description
-------|--------------------------|---------------------------------------------
400    | malformed_request        | Request body is not valid JSON.
400    | document_missing         | Document hash field missing.
400    | identity_missing         | Sharing identity field missing.
400    | share_missing            | Share targets field missing.
400    | share_identity_missing   | One or more share identity fields missing.
400    | share_signature_missing  | One or more share signature do not match.
400    | share_expiration_invalid | Expiration is not a valid UNIX timestamp.
404    | unknown_document         | Document hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "error": "unknown_document"
}
```
</details>
