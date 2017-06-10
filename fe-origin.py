import re
import sys

def readtemplate(fi, fields):
    macro = re.compile(r'%x\[(?P<row>[\d-]+),(?P<col>[\d]+)\]')
    sep=' '
    d = {}
    for index, field in enumerate(fields.split(sep)):
        d[str(index)] = field
    T = []
    for line in fi:
        line = line.strip()
        if line.startswith('#'):
            continue
        if line.startswith('U'):
            temps = tuple(re.findall(macro, line.replace(':', '=')))
            if len(temps) == 0:
                continue
            else:
                U = []
                for temp in temps:
                    U.append((d[temp[1]], int(temp[0])))
                T.append(tuple(U))
        elif line == 'B':
            continue
        elif line.startswith('B'):
            sys.stderr(
                'ERROR: bigram templates not supported: %s\n' % line)
            sys.exit(1)
    return tuple(T)

def apply_templates(X, templates):
    for template in templates:
        name = '|'.join(['%s[%d]' % (f, o) for f, o in template])
        for t in range(len(X)):
            values = []
            for field, offset in template:
                p = t + offset
                if p not in range(len(X)):
                    values = []
                    break
                values.append(X[p][field])
            if values:
                X[t]['F'].append('%s=%s' % (name, '|'.join(values)))

def readiter(fi, names, sep='\t'):
    X = []
    for line in fi:
        line = line.strip('\n')
        if not line:
            yield X
            X = []
        else:
            fields = line.split(sep)
            if len(fields) < len(names):
                print len(fields)
                print len(names)
                raise ValueError(
                    'Too few fields (%d) for %r\n%s' % (len(fields), names, line))
            item = {'F': []}    # 'F' is reserved for features.
            for i in range(len(names)):
                item[names[i]] = fields[i]
            X.append(item)

def escape(src):
    return src.replace(':', '__COLON__')

def output_features(fo, X, field=''):
    for t in range(len(X)):
        if field:
            fo.write('%s' % X[t][field])
        for a in X[t]['F']:
            if isinstance(a, str):
                fo.write('\t%s' % escape(a))
            else:
                fo.write('\t%s:%f' % (escape(a[0]), a[1]))
        fo.write('\n')
    fo.write('\n')

if __name__ == '__main__':
    
    tf = file('C:/data/template', 'r')
    fields = 'w pos y'
    template = readtemplate(tf, fields)
    
    fi = file('C:/data/test.txt', 'r')
    fo = open('C:/data/test_features.txt', 'w')
    
    for X in readiter(fi, fields.split(' '), ' '):
        apply_templates(X, template)
        output_features(fo, X, 'y')
    
    