# API Reference

> [!NOTE]
> Examples in this document use the following Ed25519 key:\
> **Private key:** `"uExw3pnNoVhz7pk7efAA6lEllKbQTEeJS4qnzxv-uBk"`\
> **Public key:** `"5uUg7dmfzRLUJmfq2xt8GOTHkjuD6iVttcL0wrGpgOc"`\
> **Hash:** `"V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs"`

**Possible Error Codes:**
Status | Error Code       | Description
-------|------------------|-----------------------------------------------------
404    | invalid_endpoint | Invalid endpoint.
500    | unexpected_error | Something went wrong with the server.


## Server Information

Get server information.

### Endpoint

```
GET /api/v1/server/info
```

### Request

<details>
<summary>Example Request</summary>

```
GET /api/v1/server/info HTTP/1.1
```
</details>

### Response

#### Success

**Body:**
Field     | Type    | Description
----------|---------|-----------------------------------------------------------
timestamp | integer | Current UNIX timestamp in seconds.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "timestamp": 1608726896
}
```
</details>


## Register Identity

Register a new anonymous identity.

The server may decide to delete anonymous identities at any time, but they are
guaranteed to be kept alive for at least 5 minutes. Associate an identity with a
server user to persist it.

### Endpoint

```
POST /api/v1/identity
```

### Request

**Body:**
Field      | Type    | Description
-----------|---------|----------------------------------------------------------
timestamp  | integer | Current UNIX timestamp in seconds.
public_key | string  | Ed25519 public key in base64url format.
pow        | string  | `pow(sha256(public_key + timestamp))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the proof-of-work is over `sha256("5uUg7dmfzRLUJmfq2xt8GOTHkjuD6iVttcL0wrGpgOc" + "1608726896")`,
> which is `sha256("5uUg7dmfzRLUJmfq2xt8GOTHkjuD6iVttcL0wrGpgOc1608726896")`.

```json
POST /api/v1/identity HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
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
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code         | Description
-------|--------------------|---------------------------------------------------
400    | malformed_request  | Request body is not valid JSON.
400    | missing_field      | Required field missing.
400    | timestamp_invalid  | Timestamp is too far from current time.
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


## Read Identity

Read the public key for a given identity.

### Endpoint

```
GET /api/v1/identity/{hash}
```

### Request

**Path:**
Segment    | Type   | Description
-----------|--------|-----------------------------------------------------------
hash       | string | Identity hash in base64url format.

<details>
<summary>Example Request</summary>

```
GET /api/v1/identity/V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs HTTP/1.1
```
</details>

### Response

#### Success

Identity found.

**Body:**
Field      | Type   | Description
-----------|--------|-----------------------------------------------------------
public_key | string | Ed25519 public key in base64url format.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "public_key": "5uUg7dmfzRLUJmfq2xt8GOTHkjuD6iVttcL0wrGpgOc"
}
```
</details>

#### Error

**Body:**
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
400    | hash_invalid      | Hash is not valid.
404    | unknown_identity  | Identity hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "error": "unknown_identity"
}
```
</details>


## Register User

Register a new user with a username, using a previously registered identity.

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
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code              | Description
-------|-------------------------|----------------------------------------------
400    | malformed_request       | Request body is not valid JSON.
400    | missing_field           | Required field missing.
400    | timestamp_invalid       | Timestamp is too far from current time.
400    | username_invalid        | Username does not meet requirements.
400    | signature_invalid       | Signature does not match.
403    | registrations_closed    | This server does not allow to create new users.
404    | unknown_identity        | Identity hash not found.
409    | username_already_taken  | Username already in use.
409    | identity_already_paired | Identity already paired with another username.

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


## User Information

Read your user information.

### Endpoint

```
POST /api/v1/user/info
```

### Request

**Body:**
Field     | Type    | Description
----------|---------|-----------------------------------------------------------
timestamp | integer | Current UNIX timestamp in seconds.
username  | string  | Your username.
identity  | string  | Identity hash in base64url format.
signature | string  | `sign("INFO " + base64url(sha256(username)) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"INFO " + base64url(sha256("example_user")) + " " + "1608726896"`,
> which is `"INFO j3BwXiW6oAwtuKkl1I53mum4elV3uQ1TOcP-8BEeH0A 1608726896"`.

```json
POST /api/v1/user/info HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "username": "example_user",
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "signature": "NpHXCe9gUV6brlRKs-f1oMoIjxSQGpij2JOZ4AaKK9p14HbAeiYv0HrYEaxGM28d3VJvqcwUHrJvA0ti5KvdDQ"
}
```
</details>

### Response

#### Success

User information returned successfully.

**Body:**
Field      | Type    | Description
-----------|---------|----------------------------------------------------------
quota      | integer | Maximum allowed usage in bytes.
used       | integer | Approximate current usage in bytes.
expiration | integer | Account expiration UNIX timestamp in seconds (unless renewed).

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "quota": 104857600,
  "used": 4096,
  "expiration": 1737635696
}
```
</details>

#### Error

**Body:**
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
400    | malformed_request | Request body is not valid JSON.
400    | missing_field     | Required field missing.
400    | timestamp_invalid | Timestamp is too far from current time.
400    | identity_invalid  | Identity not valid for this user.
400    | signature_invalid | Signature does not match.

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


## Link Identity to User

Associate a previously created identity to an existing user.

### Endpoint

```
POST /api/v1/user/identity
```

### Request

Field             | Type    | Description
------------------|---------|---------------------------------------------------
timestamp         | integer | Current UNIX timestamp in seconds.
current_identity  | string  | Hash in base64url format of an identity already associated with the user.
new_identity      | string  | Hash in base64url format of the new identity to associate.
username          | string  | User to associate with.
current_signature | string  | `sign("LINK_IDENTITY " + base64url(sha256(base64url(sha256(username)) + new_identity)) + " " + timestamp, private_key(current_identity))`
new_signature     | string  | `sign("LINK_IDENTITY " + base64url(sha256(base64url(sha256(username)) + current_identity)) + " " + timestamp, private_key(new_identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the "current" signature is over `"LINK_IDENTITY " + base64url(sha256(base64url(sha256("example_user")) + "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8")) + " " + "1608726896"`,
> which is `"LINK_IDENTITY " + base64url(sha256("j3BwXiW6oAwtuKkl1I53mum4elV3uQ1TOcP-8BEeH0Ay4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8")) + " 1608726896"`,
> which is `"LINK_IDENTITY a4rotNE6ptJAWVIfGOfVsjAggvuuIbUBAGSirPYZo3Y 1608726896"`.
>
> The "new" signature is over `"LINK_IDENTITY " + base64url(sha256(base64url(sha256("example_user")) + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs")) + " " + "1608726896"`,
> which is `"LINK_IDENTITY " + base64url(sha256("j3BwXiW6oAwtuKkl1I53mum4elV3uQ1TOcP-8BEeH0AV7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs")) + " 1608726896"`,
> which is `"LINK_IDENTITY b46N84wP43bqgM0erbqrKbZfxbYtupmQ9COZve07Rj0 1608726896"`.

```json
POST /api/v1/user/identity HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "current_identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "new_identity": "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8",
  "username": "example_user",
  "current_signature": "_fIVMwSeiyMfEUo4T2wW0auDci_Y8TADlEbxt1LOqKxsAXDekjhQNWhT2rUIs3yTGo4HJ1jTXYpSzVh5t3G6DA",
  "new_signature": "Xmp_RGNnQHvt-JIfPDHObtUjsHv64YTdNjW_emLWw9-d5dZQvTC2v2gPJqkUFa3oOZaPfMIbiOmuJWyerB3iCw"
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
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code                | Description
-------|---------------------------|--------------------------------------------
400    | malformed_request         | Request body is not valid JSON.
400    | missing_field             | Required field missing.
400    | timestamp_invalid         | Timestamp is too far from current time.
400    | current_signature_invalid | Current signature does not match.
400    | new_signature_invalid     | New signature does not match.
400    | current_identity_invalid  | Current identity not associated with the user.
404    | unknown_identity          | New identity hash not found.
409    | identity_already_paired   | New identity already paired with another username.

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


## Unlink Identity from User

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
signature | string  | `sign("UNLINK_IDENTITY " + base64url(sha256(base64url(sha256(username)) + identity)) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"UNLINK_IDENTITY " + base64url(sha256(base64url(sha256("example_user")) + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs")) + " " + "1608726896"`,
> which is `"UNLINK_IDENTITY " + base64url(sha256("j3BwXiW6oAwtuKkl1I53mum4elV3uQ1TOcP-8BEeH0AV7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs")) + " 1608726896"`,
> which is `"UNLINK_IDENTITY b46N84wP43bqgM0erbqrKbZfxbYtupmQ9COZve07Rj0 1608726896"`.

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
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code              | Description
-------|-------------------------|----------------------------------------------
400    | malformed_request       | Request body is not valid JSON.
400    | missing_field           | Required field missing.
400    | timestamp_invalid       | Timestamp is too far from current time.
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
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code                | Description
-------|---------------------------|--------------------------------------------
400    | malformed_request         | Request body is not valid JSON.
400    | missing_field             | Required field missing.
400    | timestamp_invalid         | Timestamp is too far from current time.
400    | type_invalid              | Type is not a valid GUID.
400    | expiration_invalid        | Expiration is not a valid UNIX timestamp.
400    | signature_invalid         | Signature does not match.
400    | publish_signature_invalid | Publish signature does not match.
400    | share_identity_invalid    | One or more share identity fields is not valid.
400    | share_expiration_invalid  | One or more share expiration fields is not a valid UNIX timestamp.
400    | share_signature_invalid   | One or more share signatures do not match.
404    | unknown_identity          | Identity hash not found.

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


## Read Document

Read a document by its hash.

> [!NOTE]
> All documents are public by design. Anyone can read any document even if it
> has not been shared with them, provided that they know its hash. If you don't
> want a document to be readable by other users, you must encrypt it.

### Endpoint

```
GET /api/v1/document/{hash}?format={format}
```

### Request

**Path:**
Segment | Type   | Description
--------|--------|--------------------------------------------------------------
hash    | string | Document hash in base64url format.

**Query Parameters:**
Parameter | Description
----------|---------------------------------------------------------------------
format    | Preferred response format (optional).

**Possible Formats:**
Format | Description
-------|------------------------------------------------------------------------
json   | Returns the response as a JSON object (default).
raw    | Returns the document data as raw bytes and the type as a header.

### Response

#### Success

JSON format (default if not specified).

**Body:**
Field | Type   | Description
------|--------|----------------------------------------------------------------
type  | string | Document type GUID.
data  | string | Document data in base64url format.

<details>
<summary>Example Response (JSON)</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "type": "826eca95-0078-434e-b93a-8af087da1a16",
  "data": "SGVsbG8sIFdvcmxkIQ"
}
```
</details>

Raw format.

**Headers:**
Header          | Description
----------------|---------------------------------------------------------------
X-Document-Type | Type GUID.

**Body:**
Raw document data (binary).

<details>
<summary>Example Response (Raw)</summary>

```
HTTP/1.1 200 OK
Content-Type: application/octet-stream
X-Document-Type: 826eca95-0078-434e-b93a-8af087da1a16

Hello, World!
```
</details>

#### Error

Errors are always in JSON format.

**Body:**
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
404    | unknown_document  | Document hash not found.

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


## Rent or Share Document

Rent a document for yourself, or share it with other identities.

### Endpoint

```
POST /api/v1/document/rent
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
POST /api/v1/document/rent HTTP/1.1
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
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code               | Description
-------|--------------------------|---------------------------------------------
400    | malformed_request        | Request body is not valid JSON.
400    | missing_field            | Required field missing.
400    | timestamp_invalid        | Timestamp is too far from current time.
400    | document_invalid         | Document hash is not valid.
400    | identity_invalid         | Sharing identity is not valid.
400    | share_invalid            | Share field is not valid.
400    | share_identity_invalid   | One or more share identity fields invalid.
400    | share_expiration_invalid | One or more share expiration fields is not a valid UNIX timestamp.
400    | share_signature_invalid  | One or more share signatures do not match.
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


## Unrent or Unshare Document

Forget a document by unrenting it or cancelling a share.

### Endpoint

```
DELETE /api/v1/document
```

### Request

**Body:**
Field     | Type     | Description
----------|----------|-----------------------------------------------------------
timestamp | integer  | Current UNIX timestamp in seconds.
identity  | string   | Identity hash in base64url format.
document  | string   | Document hash in base64url format.
targets   | string[] | Array of target identities (optional, defaults to self).
signature | string   | `sign("UNRENT " + base64url(sha256(document + concat(targets))) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"UNRENT " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q" + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs" + "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8") + " " + "1608726896"`
> which is `"UNRENT hZLYIzDlEf3NcwLa57gZEUyv-OxRcpHZgd_nyQsh1C8 1608726896"`.
>
> If no targets are defined, use `""`.

```json
DELETE /api/v1/document HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "document": "RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q",
  "targets": [
    "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
    "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8"
  ],
  "signature": "5Lap55bDBVi2AaFPhv-VrFHDDwrWAmZCvTDoJso8n7gpWZPmsUEkT4xm2wmvQ-g37zIAG7LYo5dvEiWOHw2wDg"
}
```
</details>

### Response

#### Success

Document unrented successfully.

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
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
400    | malformed_request | Request body is not valid JSON.
400    | missing_field     | Required field missing.
400    | timestamp_invalid | Timestamp is too far from current time.
400    | document_invalid  | Document hash is not valid.
400    | targets_invalid   | Targets field is not valid.
400    | signature_invalid | Signature does not match.
404    | unknown_identity  | Identity hash not found.
404    | unknown_document  | Document hash not found.

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


## Update Expiration

Change the expiration of a document rent for yourself or for specific shares.

### Endpoint

```
POST /api/v1/document/expiration
```

### Request

**Body:**
Field      | Type     | Description
-----------|----------|---------------------------------------------------------
timestamp  | integer  | Current UNIX timestamp in seconds.
identity   | string   | Identity hash in base64url format.
document   | string   | Document hash in base64url format.
expiration | integer  | New expiration UNIX timestamp in seconds (optional, null to remove).
targets    | string[] | Array of target identities (optional, defaults to self).
signature  | string   | `sign("SET_EXPIRATION " + base64url(sha256(document + concat(targets) + expiration)) + " " + timestamp, private_key(identity))`

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"SET_EXPIRATION " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q" + "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs" + "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8" + "1737635696")) + " " + "1608726896"`,
> which is `"SET_EXPIRATION " + base64url(sha256("RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0QV7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTsy4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc81737635696")) + " 1608726896"`
> which is `"SET_EXPIRATION nyTzqxORMXUFZZbDzZtzQzma6LNSojGwSw2Ht0APquU 1608726896"`.
>
> When the expiration is not set, use `""` in its place.
>
> If no targets are defined, use `""`.

```json
POST /api/v1/document/expiration HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "document": "RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q",
  "expiration": 1737635696,
  "targets": [
    "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
    "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8"
  ],
  "signature": "1saxMK4lbsRovdsfqO1uMADQdfa4zt0CojbPcJmS0blRBr1O0F_HHDVbXmHXTVdjJkdk-6Tt-FrlrkiRRpSwDA"
}
```
</details>

### Response

#### Success

Expiration changed successfully.

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
------|--------|---------------------------
error | string | Error code.

**Possible Error Codes:**
Status | Error Code           | Description
-------|----------------------|--------------------------
400    | malformed_request    | Request body is not valid JSON.
400    | missing_field        | Required field missing.
400    | timestamp_invalid    | Timestamp is too far from current time.
400    | identity_invalid     | Identity is not valid.
400    | document_invalid     | Document hash is not valid.
400    | expiration_invalid   | Expiration is not a valid UNIX timestamp.
400    | targets_invalid      | Targets field is not valid.
400    | signature_invalid    | Signature does not match.
404    | unknown_identity     | Identity hash not found.
404    | unknown_document     | Document hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "expiration_invalid"
}
```
</details>

## Listen for Documents

Listen for new documents shared with an identity.

Listening can be done either synchronously or asynchronously. By listening
synchronously it's possible to receive all documents in real-time as they are
shared with your identity. By listening asynchronously it's possible to fetch
the new documents that have been shared with you identity since last time.

### Endpoint

```
POST /api/v1/document/listen?timeout={timeout}
```

### Request

**Body:**
Field     | Type     | Description
----------|----------|----------------------------------------------------------
timestamp | integer  | Current UNIX timestamp in seconds.
identity  | string   | Identity hash in base64url format.
types     | string[] | Array of document type GUIDs to listen for.
signature | string   | `sign("LISTEN " + base64url(sha256(concat(types))) + " " + timestamp, private_key(identity))`
cursor    | string   | Opaque cursor for pagination/continuation (optional).

**Query Parameters:**
Parameter | Description
----------|---------------------------------------------------------------------
timeout   | Timeout in seconds (optional).

If a timeout is specified, the server will try to keep the connection open for
at most the requested amount of time before giving up waiting for new documents.

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"LISTEN " + base64url(sha256("826eca95-0078-434e-b93a-8af087da1a16" + "e0386c32-9b6b-42c0-bf1a-7f81793ad96a")) + " " + "1608726896"`
> which is `"LISTEN YCrEjPzcVbObjBjSZmZpEDuihHjv5mqr2sunLFVW1do 1608726896"`.

```json
POST /api/v1/document/listen HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "types": [
    "826eca95-0078-434e-b93a-8af087da1a16",
    "e0386c32-9b6b-42c0-bf1a-7f81793ad96a"
  ],
  "signature": "xevrjGs09jWZnPQ7spUoAALoS-wTlmx_NLTscF7C9AdsqXcxc0EOMbj5MlZMwRgaaVvDXNo_9XiYarI2hWG1Dg",
  "cursor": "VGhpcyBpcyBhbiBvcGFxdWUgY3Vyc29yLCBwbGVhc2UgZG8gbm90IHRyeSB0byBkZWNvZGUgaXQuIDop"
}
```
</details>

You may also listen for documents over a WebSocket connection. Endpoint, request
payload and response format remain the same. The request payload should be sent
as the first message, and the response will be sent as a websocket message.

As long as the connection will remain open, the server will keep sending new
documents as messages.

The timeout query parameter is ignored when making a WebSocket request.

### Response

#### Success

**Body:**
Field   | Type     | Description
--------|----------|------------------------------------------------------------
hashes  | string[] | Array of document hashes in base64url format.
cursor  | string   | Opaque cursor for fetching the next batch.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "hashes": [
    "RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q",
    "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8"
  ],
  "cursor": "VGhpcyBpcyBhbm90aGVyIG9wYXF1ZSBjdXJzb3IsIGl0cyBjb250ZW50IGlzIG9ubHkgbWVhbmluZ2Z1bCB0byB0aGUgc2VydmVyLg"
}
```
</details>

<details>
<summary>Example WebSocket Message</summary>

```json
{
  "hashes": [
    "RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q",
    "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8"
  ],
  "cursor": "VGhpcyBpcyBhbm90aGVyIG9wYXF1ZSBjdXJzb3IsIGl0cyBjb250ZW50IGlzIG9ubHkgbWVhbmluZ2Z1bCB0byB0aGUgc2VydmVyLg"
}
```
</details>

#### Error

**Body:**
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
400    | malformed_request | Request body is not valid JSON.
400    | missing_field     | Required field missing.
400    | timestamp_invalid | Timestamp is too far from current time.
400    | identity_invalid  | Identity is not valid.
400    | types_invalid     | Types field is not valid.
400    | signature_invalid | Signature does not match.
400    | cursor_invalid    | Cursor field is not valid.
404    | unknown_identity  | Identity hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "missing_field"
}
```
</details>


## List Document Types

List all document types for which there exists at least one document counted
towards your user quota, this includes documents rented by your identity and
documents shared **with** other identities, but it does not include documents
shared **by** other identities unless they have also been rented by your
identity.

### Endpoint

```
POST /api/v1/document/type/list
```

### Request

**Body:**
Segment   | Type    | Description
----------|---------|-----------------------------------------------------------
timestamp | integer | Current UNIX timestamp in seconds.
identity  | string  | Identity hash in base64url format.
signature | string  | `sign("LIST_TYPES " + timestamp, private_key(identity))`
cursor    | string  | Opaque cursor for pagination/continuation (optional).

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"LIST_TYPES " + "1608726896"`,
> which is `"LIST_TYPES 1608726896"`.

```json
POST /api/v1/document/type/list HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "signature": "r-5Th5KyQaTMk74AvcmJVnmUuxurrBjrErZqsnFX-lz6PF3tHaFMP25q4MUCoV1rhEDGEvgPvR6GuFIubHG3CQ",
  "cursor": null
}
```
</details>

### Response

#### Success

**Body:**
Field  | Type     | Description
-------|----------|-------------------------------------------------------------
types  | string[] | Array of document type GUIDs.
cursor | string   | Opaque cursor for pagination/continuation (null if finished).

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "types": [
    "826eca95-0078-434e-b93a-8af087da1a16",
    "e0386c32-9b6b-42c0-bf1a-7f81793ad96a"
  ],
  "cursor": "UGFzcyB0aGlzIGN1cnNvciB0byByZWFkIHRoZSBuZXh0IHBhZ2Uu"
}
```
</details>

#### Error

**Body:**
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
400    | missing_field     | Required field missing.
400    | timestamp_invalid | Timestamp is too far from current time.
400    | identity_invalid  | Identity is not valid.
400    | signature_invalid | Signature does not match.
400    | cursor_invalid    | Cursor field is not valid.
404    | unknown_identity  | Identity hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "error": "unknown_identity"
}
```
</details>


## List Documents

List all document counted towards your user quota, this includes documents
rented by your identity and documents shared **with** other identities, but it
does not include documents shared **by** other identities unless they have also
been rented by your identity.

### Endpoint

```
POST /api/v1/document/list
```

### Request

**Body:**
Segment   | Type     | Description
----------|----------|----------------------------------------------------------
timestamp | integer  | Current UNIX timestamp in seconds.
identity  | string   | Identity hash in base64url format.
types     | string[] | Array of document type GUIDs.
signature | string   | `sign("LIST " + base64url(sha256(concat(types))) + " " + timestamp, private_key(identity))`
cursor    | string   | Opaque cursor for pagination/continuation (optional).

<details>
<summary>Example Request</summary>

> [!NOTE]
> In this example, the signature is over `"LIST " + base64url(sha256("826eca95-0078-434e-b93a-8af087da1a16" + "e0386c32-9b6b-42c0-bf1a-7f81793ad96a")) + " " + "1608726896"`
> which is `"LIST YCrEjPzcVbObjBjSZmZpEDuihHjv5mqr2sunLFVW1do 1608726896"`.

```json
POST /api/v1/document/list HTTP/1.1
Content-Type: application/json

{
  "timestamp": 1608726896,
  "identity": "V7hZQY0g61dMbywtkhZyIkXnU-wNBENi9xFFSX0qzTs",
  "types": [
    "826eca95-0078-434e-b93a-8af087da1a16",
    "e0386c32-9b6b-42c0-bf1a-7f81793ad96a"
  ],
  "signature": "y_-jz2PdwEM8TaDY65N24rXl24_KbaD0nhPegEU4rr6Teq40sYe9_5TZi7W2tJIpc1ZfSWrjlgsp6HWHo9PIBA",
  "cursor": null
}
```
</details>

### Response

#### Success

**Body:**
Field  | Type     | Description
-------|----------|-------------------------------------------------------------
hashes | string[] | Array of document hashes in base64url format.
cursor | string   | Opaque cursor for pagination/continuation (null if finished).

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "hashes": [
    "RlzbiZkTdKO-5_mRng8zlsHXxNXh81ZV-5fLE1XyV0Q",
    "y4dr5PwoEpKYlJS8OojzcVgN0UI_NH8NRTVo5b3tAc8"
  ],
  "cursor": "VXNlIHRoaXMgY3Vyc29yIGluIHRoZSBuZXh0IGNhbGwgdG8gZ2V0IHRoZSBuZXh0IHBhZ2Uu"
}
```
</details>

#### Error

**Body:**
Field       | Type   | Description
------------|--------|----------------------------------------------------------
error       | string | Error code.
description | string | Human-readable description of the error (optional).

**Possible Error Codes:**
Status | Error Code        | Description
-------|-------------------|----------------------------------------------------
400    | missing_field     | Required field missing.
400    | timestamp_invalid | Timestamp is too far from current time.
400    | identity_invalid  | Identity is not valid.
400    | types_invalid     | Types field is not valid.
400    | signature_invalid | Signature does not match.
400    | cursor_invalid    | Cursor field is not valid.
404    | unknown_identity  | Identity hash not found.

<details>
<summary>Example Response</summary>

```json
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "error": "unknown_identity"
}
```
</details>
