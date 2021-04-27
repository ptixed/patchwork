
# Patchwork

Tool for managing kubernetes configuration a'la kustomize.
Based on given input file it renders configuration by applying patches to set of kubernetes objects.

Input file structure:

```yml

# ---------------------------
# constant value identifying this file as patchwork input file
kind: Patchwork


# ---------------------------
# list of paths to be included in the rendering process
includes:

# path can point to kubernetes objects
- objects/deployment.yml
# simple wildcards ('*' and '?') are supported
# this will load all yml files directly in azure-definitions directory
- azure-definitions/*.yml
# path can also point to other patchwork file,
# in this case '../prod/patchwork.yml' will be rendered using the same general logic as this file
- ../prod/patchwork.yml


# ---------------------------
# for files loaded using 'includes' patches can be applied
patches:

# ---------------------------
# 'match' specifies what objects to apply changes to
# program looks through loaded objects and matches only the ones with matching structure and values
# this example will select all CronJobs
- match:
    # values are actually anchored regexes
    # in this case 'kind' is actually expected to match '^CronJob$'
    kind: CronJob

# patch specifies what properties to add or replace on matched object
# arrays are concatenated by default
# in this example all CronJobs will be suspended
  patch:
    spec:
      suspend: true


# more complex examples:

# ---------------------------
# this will match all deployments with name starting with 'rest-'
- match:
    kind: Deployment
    metadata:
      name: rest-.*
# number of replicas will be set to 2
  patch:
    spec:
      replicas: 2

# ---------------------------
# for all Ingresses element in 'spec.tls' will be added
- match:
    kind: Ingress
  patch:
    spec:
      tls:
      - secretName: example-tls-secret
        hosts:
        - example.com

# ---------------------------
# modifying array element instead of concatenating

# for this purpose 'patchPath' property is used
# it selects by a means of json-path expression nodes in matched object that the patch will be applied to
# (instead of by default applying changes to object's root)
# 'patchPath' can evaluate to multiple items - patches will be applied to all of them
# in this example for all Ingresses we select their spec.rules and for each of them set value of 'host'
- match:
    kind: Ingress
  patchPath: spec.rules[*]
  patch:
    host: example.com

```
