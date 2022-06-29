# ndot

DNS over [DoT](https://en.wikipedia.org/wiki/DNS_over_TLS) relay. Designed for DHCP-configurable routers that supports custom DNS servers.

![](gitm/prev.PNG)

## Features
 
| Feature | Description |
| :----- | :------ |
| DNS[^1] (port 53) to DoT[^2] conversion | Wraps and relays incoming DNS request towards DoT as per RFC1035 и RFC7858 |
| Response display | Logs DoT responses as domain/IP-address |
| Overridable DoT-server (fork/lib-mode) | Target DoT server can be overrided either in fork or lib mode |

## \[RU\]

Ретранслятор DNS в DoT для DHCP-конфигурируемых роутеров с перегружаемым DNS.

## Возможности
   
| Возможность | Описание |
| :----- | :------ |
| Конверсия DNS[^1] (порт 53) в DoT[^2] | Оборачивает и перенаправляет входящий DNS-запрос в DoT по стандартам RFC1035 и RFC7858 |
| Отображение ответов | Выводит в консоль ответ DoT-cервера вида домен/IP-адрес |
| Переопределяемый DoT-сервер (fork/lib-mode) | Возможность переопределения DoT-сервера разработчиком непосредственно |

## References:

[^1]: Internet Engineering Task Force (IETF). RFC1035. DOMAIN NAMES - IMPLEMENTATION AND SPECIFICATION - https://datatracker.ietf.org/doc/html/rfc1035
[^2]: Internet Engineering Task Force (IETF). RFC7858. Specification for DNS over Transport Layer Security (TLS) - https://datatracker.ietf.org/doc/html/rfc7858