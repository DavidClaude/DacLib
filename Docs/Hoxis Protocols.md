# Protocols

## Req. & Resp.

##### ● SignIn

Requests for becoming a user of HoxisServer with its user ID

| Protocols | Arguments  | Description                   |
| --------- | ---------- | ----------------------------- |
| Req.      | (long uid) | uid: which AuthServer returns |
| Resp.     | (int code) | code: success or reconnect    |

##### ● GetRealtimeStatus

Requests for getting HoxisRealtimeStatus which the server keeps, it's useful to recover the status of user who is disconnected exceptionally, such as team status, playing status

| Protocols | Arguments                             | Description                            |
| --------- | ------------------------------------- | -------------------------------------- |
| Req.      | ()                                    | -                                      |
| Resp.     | (int code,HoxisRealtimeStatus status) | status: such as UserState, player data |

##### ● LoadUserData

Requests for loading the user data bound with user ID in the database, which is used for get the user's appearance, assets and statisitcs

| Protocols | Arguments                | Description                                       |
| --------- | ------------------------ | ------------------------------------------------- |
| Req.      | ()                       | -                                                 |
| Resp.     | (int code,UserData data) | data: user's appearance, assets and statisitcs... |

##### ● SaveUserData

Requests for saving the user data updated to the database

| Protocols | Arguments       | Description                          |
| --------- | --------------- | ------------------------------------ |
| Req.      | (UserData data) | data: only push partial data updated |
| Resp.     | (int code)      | -                                    |

